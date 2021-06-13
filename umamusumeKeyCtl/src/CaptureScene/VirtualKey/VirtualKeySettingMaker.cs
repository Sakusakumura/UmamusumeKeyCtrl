using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using umamusumeKeyCtl.UserInput;

namespace umamusumeKeyCtl.CaptureScene
{
    public class VirtualKeySettingMaker
    {
        private List<VirtualKeySetting> _settings;
        public event Action<List<VirtualKeySetting>> SettingCreated;

        private UIElement _eventListeningSource;
        
        private VirtualKeySettingMakingState _state = VirtualKeySettingMakingState.Waiting;

        private VirtualKeySettingMakingState state
        {
            get => _state;
            set
            {
                _state = value;
                OnStateChange(_state);
            }
        }

        private Point _point = new Point();
        private Key _key = Key.A;
        private bool _drawCircle = false;
        private Canvas _canvas;
        private List<UIElement> _uiElements = new ();
        private LowLevelKeyboardListener _keyboardListener;

        public VirtualKeySettingMaker(Canvas canvas, UIElement eventListeningSource, bool drawCircle, VirtualKeySetting[] settings = null)
        {
            _settings = new List<VirtualKeySetting>();

            if (settings != null)
            {
                _settings.AddRange(settings);
            }
            
            _eventListeningSource = eventListeningSource;
            _eventListeningSource.MouseLeftButtonUp += OnMouseLeftUp;

            _drawCircle = drawCircle;
            _canvas = canvas;

            state = VirtualKeySettingMakingState.GetPosTo;
        }

        public void Cancel()
        {
            state = VirtualKeySettingMakingState.Waiting;
            
            if (_drawCircle)
            {
                foreach (var uiElement in _uiElements)
                {
                    _canvas.Children.Remove(uiElement);
                }
                _uiElements.Clear();
            }
        }

        private void OnMouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            if (state != VirtualKeySettingMakingState.GetPosTo)
            {
                return;
            }
            
            _point = e.GetPosition(_eventListeningSource);

            state = VirtualKeySettingMakingState.GetBindKey;
        }

        private void OnKeyPressed(object _, KeyPressedArgs args)
        {
            if (state != VirtualKeySettingMakingState.GetBindKey)
            {
                return;
            }
            
            if (_settings.Count > 0 && _settings.Exists(val => val.BindKey == args.KeyPressed))
            {
                return;
            }
            
            _key = args.KeyPressed;
                
            if (_drawCircle)
            {
                var brushConverter = new BrushConverter();
                var element = new Grid();
                element.Children.Add(new Ellipse()
                {
                    Width = 20,
                    Height = 20,
                    Stroke = (SolidColorBrush) brushConverter.ConvertFromString("#e9eaea"),
                    Fill = (SolidColorBrush) brushConverter.ConvertFromString("#484b49")
                });
                element.Children.Add(new Label()
                {
                    Width = 20,
                    Height = 20,
                    FontSize = 9,
                    Content = $"{_key.ToString()}",
                    Foreground = (SolidColorBrush) brushConverter.ConvertFromString("#e9eaea"),
                    Background = (SolidColorBrush) brushConverter.ConvertFromString("Transparent"),
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                });

                var left = _point.X - 10;
                var top = _point.Y - 10;
                    
                _canvas.Children.Add(element);
                Canvas.SetLeft(element, left);
                Canvas.SetTop(element, top);
                    
                _uiElements.Add(element);
            }
            
            _keyboardListener.OnKeyPressed -= OnKeyPressed;
            _keyboardListener.UnHookKeyboard();
                
            state = VirtualKeySettingMakingState.Create;
        }

        private void OnStateChange(VirtualKeySettingMakingState state)
        {
            if (state == VirtualKeySettingMakingState.GetPosTo)
            {
                new MessageWindow("押す場所をクリックしてください").ShowDialog();
                return;
            }

            if (state == VirtualKeySettingMakingState.GetBindKey)
            {
                new MessageWindow("割り当てるキーを入力してください").ShowDialog();

                _keyboardListener = new LowLevelKeyboardListener();
                _keyboardListener.HookKeyboard();
                _keyboardListener.OnKeyPressed += OnKeyPressed;
                
                return;
            }

            if (state == VirtualKeySettingMakingState.Create)
             {
                _settings.Add(new VirtualKeySetting(GetNewIndex(_settings), _key, _point));

                if (new YesNoWindow("続けて設定しますか？").ShowDialog().Value)
                {
                    this.state = VirtualKeySettingMakingState.GetPosTo;
                    
                    return;
                }

                if (_drawCircle)
                {
                    foreach (var uiElement in _uiElements)
                    {
                        _canvas.Children.Remove(uiElement);
                    }
                    _uiElements.Clear();
                }
                
                _eventListeningSource.MouseLeftButtonUp -= OnMouseLeftUp;
                
                SettingCreated?.Invoke(_settings);
            }
        }
        
        private int GetNewIndex(List<VirtualKeySetting> infos)
        {
            int temp;

            if (infos.Count == 0)
            {
                return 0;
            }

            var maxIndex = infos.Select(val => val.Index).Max();

            for (temp = 0; temp < maxIndex; temp++)
            {
                bool flag = false;
                
                foreach (var info in infos)
                {
                    if (info.Index == temp)
                    {
                        flag = true;
                    }
                }

                if (flag)
                {
                    continue;
                }
                
                break;
            }

            if (temp == maxIndex)
            {
                temp += 1;
            }

            return temp;
        }
        
        private enum VirtualKeySettingMakingState
        {
            Waiting,
            GetPosTo,
            GetBindKey,
            Create,
            Created,
        }
    }
}