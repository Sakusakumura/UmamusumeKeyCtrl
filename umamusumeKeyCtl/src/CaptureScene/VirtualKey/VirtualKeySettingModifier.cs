using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using umamusumeKeyCtl.UserInput;

namespace umamusumeKeyCtl.CaptureScene
{
    public class VirtualKeySettingModifier
    {
        private List<VirtualKeySetting> _virtualKeySettings = new ();
        private List<UIElement> _elements = new ();
        private EditState _state = EditState.Waiting;
        private Grid _selecting;
        private LowLevelKeyboardListener _keyboardListener;
        private Canvas _canvas;
        private VirtualKeySettingMaker _maker;
        private int _targetIndex;
        private VirtualKeySetting _selectingSetting;

        public event Action<List<VirtualKeySetting>> ChangeVirtualKeys;
        
        public VirtualKeySettingModifier(List<VirtualKeySetting> keySettings, Canvas canvas)
        {
            _virtualKeySettings = keySettings;

            _canvas = canvas;

            foreach (var virtualKeySetting in keySettings)
            {
                DrawVirtualKeySettings(virtualKeySetting, _canvas);
            }
        }

        public void Repaint()
        {
            Discard();
            foreach (var virtualKeySetting in _virtualKeySettings)
            {
                DrawVirtualKeySettings(virtualKeySetting, _canvas);
            }
        }

        public void Discard()
        {
            foreach (var uiElement in _elements)
            {
                _canvas.Children.Remove(uiElement);
            }
            
            _elements.Clear();
        }
        
        public void OnEditModeChanged(EditMode mode)
        {
            if (mode == EditMode.Modify)
            {
                if (_state == EditState.Adding)
                {
                    _maker.Cancel();
                    _maker.SettingCreated -= OnSettingCreatedNewly;
                    _maker = null;
                }
                _state = EditState.Waiting;
                return;
            }
            if (mode == EditMode.Add)
            {
                _maker = new VirtualKeySettingMaker(_canvas, _canvas, true, _virtualKeySettings.ToArray());
                _maker.SettingCreated += OnSettingCreatedNewly;
                _state = EditState.Adding;
                return;
            }

            if (mode == EditMode.Remove)
            {
                if (_state == EditState.Adding)
                {
                    _maker.Cancel();
                    _maker.SettingCreated -= OnSettingCreatedNewly;
                    _maker = null;
                }
                _state = EditState.Removing;
                return;
            }
        }

        private void DrawVirtualKeySettings(VirtualKeySetting setting, Canvas canvas)
        {
            var brushConverter = new BrushConverter();
            var element = new Grid();
            var ellipse = new Ellipse()
            {
                Width = 25,
                Height = 25,
                Stroke = (SolidColorBrush) brushConverter.ConvertFromString("#e9eaea"),
                Fill = (SolidColorBrush) brushConverter.ConvertFromString("#484b49"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Focusable = false,
            };
            element.Children.Add(ellipse);
            var label = new Label()
            {
                Width = 25,
                FontSize = 13,
                Content = $"{setting.BindKey.ToString()}",
                Foreground = (SolidColorBrush) brushConverter.ConvertFromString("#e9eaea"),
                Background = (SolidColorBrush) brushConverter.ConvertFromString("Transparent"),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Focusable = true,
            };
            element.Children.Add(label);

            label.MouseLeftButtonDown += (_, args) =>
            {
                label.Focusable = false;
                OnMouseLeftDown(element, args, setting);
            };
            canvas.MouseLeftButtonUp += (obj, args) =>
            {
                label.Focusable = true;
                OnMouseLeftUp((Canvas) obj, args);
            };
            canvas.MouseMove += (obj, args) => OnMouseMove((Canvas) obj, element, args);
            label.MouseRightButtonUp += (_, _) => OnMouseRightUp(label, ellipse, setting);
            label.MouseLeftButtonUp += (_, _) => OnRemoveSettingSelected(setting);

            var left = setting.PressPos.X - 10;
            var top = setting.PressPos.Y - 10;
                    
            canvas.Children.Add(element);

            _elements.Add(element);
            
            Canvas.SetLeft(element, left);
            Canvas.SetTop(element, top);
        }

        private void OnSettingCreatedNewly(List<VirtualKeySetting> settings)
        {
            if (settings == null || settings.Count == 0)
            {
                return;
            }
            
            _virtualKeySettings = settings;
            
            ChangeVirtualKeys?.Invoke(_virtualKeySettings);

            _state = EditState.Waiting;
        }

        private void OnRemoveSettingSelected(VirtualKeySetting setting)
        {
            if (_state != EditState.Removing)
            {
                return;
            }

            var target = _virtualKeySettings.Find(val => val.Index == setting.Index);
            _virtualKeySettings.Remove(target);
            
            ChangeVirtualKeys?.Invoke(_virtualKeySettings);
        }

        private void OnMouseLeftDown(Grid sender, MouseButtonEventArgs args, VirtualKeySetting setting)
        {
            if (_state != EditState.Waiting)
            {
                return;
            }
            
            _selecting = sender;
            _selectingSetting = setting;
            _targetIndex = setting.Index;
            _state = EditState.Moving;
        }

        private void OnMouseMove(Canvas obj, Grid sender, MouseEventArgs args)
        {
            if (_state != EditState.Moving || _selecting != sender)
            {
                return;
            }

            var pos = args.GetPosition(obj);
            
            Canvas.SetLeft(sender, pos.X - 10);
            Canvas.SetTop(sender, pos.Y - 10);
        }

        private void OnMouseLeftUp(Canvas sender, MouseButtonEventArgs args)
        {
            if (_state != EditState.Moving)
            {
                return;
            }

            _virtualKeySettings.Remove(_virtualKeySettings.Find(val => val.Index == _targetIndex));
            
            _virtualKeySettings.Add(new VirtualKeySetting(_targetIndex, _selectingSetting.BindKey, args.GetPosition(sender)));
            
            ChangeVirtualKeys?.Invoke(_virtualKeySettings);

            _state = EditState.Waiting;
        }

        private void OnMouseRightUp(Label label, Ellipse ellipse, VirtualKeySetting setting)
        {
            if (_state != EditState.Waiting)
            {
                return;
            }
            
            _keyboardListener = new LowLevelKeyboardListener();
            _keyboardListener.HookKeyboard();
            _keyboardListener.OnKeyPressed += (_, args) => OnKeyPressed(label, ellipse, args, setting);
            _state = EditState.Binding;

            var converter = new BrushConverter();
            label.Content = "？";
            label.Foreground = (Brush) converter.ConvertFromString("#0c0d0c");
            ellipse.Fill = (Brush) converter.ConvertFromString("#e9eaea");
            ellipse.Stroke = (Brush) converter.ConvertFromString("#484b49");
        }

        private void OnKeyPressed(Label label, Ellipse ellipse, KeyPressedArgs args, VirtualKeySetting setting)
        {
            if (_state != EditState.Binding)
            {
                return;
            }

            if (_virtualKeySettings.Count > 0 && _virtualKeySettings.Exists(val => val.BindKey == args.KeyPressed))
            {
                return;
            }

            var converter = new BrushConverter();
            label.Content = args.KeyPressed.ToString();
            label.Foreground = (Brush) converter.ConvertFromString("#e9eaea");
            ellipse.Fill = (Brush) converter.ConvertFromString("#484b49");
            ellipse.Stroke = (Brush) converter.ConvertFromString("#e9eaea");

            var newSetting = new VirtualKeySetting(setting.Index, args.KeyPressed, setting.PressPos);

            _virtualKeySettings.Remove(_virtualKeySettings.Find(val => val.Index == setting.Index));
            _virtualKeySettings.Add(newSetting);
            
            ChangeVirtualKeys?.Invoke(_virtualKeySettings);

            _keyboardListener.UnHookKeyboard();
            _keyboardListener = null;

            _state = EditState.Waiting;
        }

        private enum EditState
        {
            Waiting,
            Adding,
            Removing,
            Moving,
            Binding,
        }
    }
}