using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using umamusumeKeyCtl.Annotations;
using umamusumeKeyCtl.Helpers;
using Brush = System.Drawing.Brush;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;

namespace umamusumeKeyCtl.UserInput
{
    public class RectangleGetter
    {
        private CaptureState _captureState = CaptureState.Capturing_pos1;
        private Point _point1;
        private Point _point2;
        private Canvas _canvas;
        private UIElement _uiElement;
        private System.Windows.Shapes.Rectangle _rectangle;
        private bool _drawRectangle;
        private System.Windows.Media.Brush _rightBrush;
        private System.Windows.Media.Brush _leftBrush;

        public event Action<Rect> OnGetRectangle; 

        public RectangleGetter(Canvas canvas, UIElement eventListenSource, bool drawRectangle, SolidColorBrush rightButtonBrush = null, System.Windows.Media.Brush leftButtonBrush = null)
        {
            new MessageWindow("領域をドラッグ&ドロップで指定して下さい").ShowDialog();
            
            _canvas = canvas;
            _uiElement = eventListenSource;
            _drawRectangle = drawRectangle;

            _rightBrush = rightButtonBrush ?? Brushes.Red;
            _leftBrush = leftButtonBrush ?? Brushes.Red;

            _uiElement.MouseLeftButtonDown += OnLeftMouseDownUp;
            _uiElement.MouseLeftButtonUp += OnLeftMouseDownUp;
            _uiElement.MouseRightButtonDown += OnRightMouseDownUp;
            _uiElement.MouseRightButtonUp += OnRightMouseDownUp;
            _uiElement.MouseMove += OnMouseMove;
        }

        public void Cancel()
        {
            _captureState = CaptureState.Captured;
        }

        public void Unload()
        {
            if (_rectangle != null)
            {
                _canvas.Children.Remove(_rectangle);
                _rectangle = null;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_captureState == CaptureState.Capturing_pos2 && _rectangle != null)
            {
                var rect = RectangleHelper.GetRect(_point1, e.GetPosition(_uiElement));
                
                _rectangle.Width = rect.Width;
                _rectangle.Height = rect.Height;
                
                Canvas.SetLeft(_rectangle,rect.X);
                Canvas.SetTop(_rectangle,rect.Y);
            }
        }

        private void OnLeftMouseDownUp(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(_uiElement);
            
            if (e.ButtonState == MouseButtonState.Pressed && _captureState == CaptureState.Capturing_pos1)
            {
                _point1 = mousePos;
                _captureState = CaptureState.Capturing_pos2;

                if (_drawRectangle)
                {
                    _rectangle = new System.Windows.Shapes.Rectangle()
                    {
                        Stroke = _leftBrush,
                        Fill = Brushes.Transparent,
                        Focusable = false
                    };
                    _canvas.Children.Add(_rectangle);

                }
                
                return;
            }

            if (e.ButtonState == MouseButtonState.Released && _captureState == CaptureState.Capturing_pos2)
            {
                _point2 = mousePos;
                _captureState = CaptureState.Captured;
                Unsubscribe();
                OnGetRectangle?.Invoke(RectangleHelper.GetRect(_point1, _point2));
                return;
            }
        }
        
        private void OnRightMouseDownUp(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(_uiElement);
            
            if (e.ButtonState == MouseButtonState.Pressed && _captureState == CaptureState.Capturing_pos1)
            {
                _point1 = mousePos;
                _captureState = CaptureState.Capturing_pos2;

                if (_drawRectangle)
                {
                    _rectangle = new System.Windows.Shapes.Rectangle()
                    {
                        Stroke = _rightBrush,
                        Fill = Brushes.Transparent,
                        Focusable = false
                    };
                    _canvas.Children.Add(_rectangle);

                }
                
                return;
            }

            if (e.ButtonState == MouseButtonState.Released && _captureState == CaptureState.Capturing_pos2)
            {
                _point2 = mousePos;
                _captureState = CaptureState.Captured;
                Unsubscribe();
                OnGetRectangle?.Invoke(RectangleHelper.GetRect(_point1, _point2));
                return;
            }
        }

        private void Unsubscribe()
        {
            _uiElement.MouseLeftButtonDown -= OnLeftMouseDownUp;
            _uiElement.MouseLeftButtonUp -= OnLeftMouseDownUp;
            _uiElement.MouseRightButtonDown -= OnRightMouseDownUp;
            _uiElement.MouseRightButtonUp -= OnRightMouseDownUp;
            _uiElement.MouseMove -= OnMouseMove;
        }
        
        private enum CaptureState
        {
            Capturing_pos1,
            Capturing_pos2,
            Captured
        }
    }
}