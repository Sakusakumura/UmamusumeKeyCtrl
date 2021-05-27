using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using umamusumeKeyCtl.UserInput;

namespace umamusumeKeyCtl.CaptureSettingSets.VirtualKeyPushing
{
    public class VirtualKeySettingMaker
    {
        private List<VirtualKeySetting> _settings = new ();
        public event Action<List<VirtualKeySetting>> OnSettingCreated;

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

        public VirtualKeySettingMaker(Canvas canvas, UIElement eventListeningSource, bool drawCircle)
        {
            _eventListeningSource = eventListeningSource;
            _eventListeningSource.MouseLeftButtonUp += OnMouseLeftUp;
            LowLevelKeyboardListener.Instance.OnKeyPressed += OnKeyPressed;

            _drawCircle = drawCircle;
            _canvas = canvas;

            state = VirtualKeySettingMakingState.GetPosTo;
        }

        private void OnMouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            if (state == VirtualKeySettingMakingState.GetPosTo)
            {
                _point = e.GetPosition(_eventListeningSource);

                state = VirtualKeySettingMakingState.GetBindKey;
            }
        }

        private void OnKeyPressed(object _, KeyPressedArgs args)
        {
            if (state == VirtualKeySettingMakingState.GetBindKey)
            {
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
                
                state = VirtualKeySettingMakingState.Create;
            }
        }

        private void OnStateChange(VirtualKeySettingMakingState state)
        {
            if (state == VirtualKeySettingMakingState.GetPosTo)
            {
                MessageBox.Show("押す場所をクリックしてください");
            }

            if (state == VirtualKeySettingMakingState.GetBindKey)
            {
                MessageBox.Show("割り当てるキーを入力してください");
            }
            
            if (state == VirtualKeySettingMakingState.Create)
             {
                _settings.Add(new VirtualKeySetting(_key, _point));

                if (MessageBox.Show("続けて設定しますか？", "Question", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
                
                OnSettingCreated?.Invoke(_settings);

                LowLevelKeyboardListener.Instance.OnKeyPressed -= OnKeyPressed;
                _eventListeningSource.MouseLeftButtonUp -= OnMouseLeftUp;
            }
        }
        
        private enum VirtualKeySettingMakingState
        {
            Waiting,
            GetPosTo,
            GetBindKey,
            Create,
        }
    }
}