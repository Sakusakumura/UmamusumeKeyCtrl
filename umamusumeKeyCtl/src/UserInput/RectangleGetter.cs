using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using umamusumeKeyCtl.Helpers;
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

        public event Action<Rectangle> OnGetRectangle; 

        public RectangleGetter(Canvas canvas, UIElement eventListenSource, bool drawRectangle)
        {
            _canvas = canvas;
            _uiElement = eventListenSource;
            _drawRectangle = drawRectangle;

            eventListenSource.MouseLeftButtonUp += OnLeftMouseDown;
            eventListenSource.MouseMove += OnMouseMove;
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
                var rect = GetRect(_point1, e.GetPosition(_uiElement));
                
                _rectangle.Width = rect.Width;
                _rectangle.Height = rect.Height;
                
                Canvas.SetLeft(_rectangle,rect.X);
                Canvas.SetTop(_rectangle,rect.Y);
            }
        }

        private void OnLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(_uiElement);
            
            if (_captureState == CaptureState.Capturing_pos1)
            {
                _point1 = mousePos;
                _captureState = CaptureState.Capturing_pos2;

                if (_drawRectangle)
                {
                    _rectangle = new System.Windows.Shapes.Rectangle()
                    {
                        Stroke = Brushes.Red,
                        Fill = Brushes.Transparent,
                        Focusable = false
                    };
                    _canvas.Children.Add(_rectangle);

                }
                
                return;
            }

            if (_captureState == CaptureState.Capturing_pos2)
            {
                _point2 = mousePos;
                _captureState = CaptureState.Captured;
                OnGetRectangle?.Invoke(GetRect(_point1, _point2));
                return;
            }
        }

        private Rectangle GetRect(Point point1, Point point2)
        {
            var minX = (int) Math.Min(point1.X, point2.X);
            var maxX = (int) Math.Max(point1.X, point2.X);
            var minY = (int) Math.Min(point1.Y, point2.Y);
            var maxY = (int) Math.Max(point1.Y, point2.Y);
            
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
        
        private enum CaptureState
        {
            Capturing_pos1,
            Capturing_pos2,
            Captured
        }
    }
}