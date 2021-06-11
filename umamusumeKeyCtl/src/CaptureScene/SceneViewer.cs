using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace umamusumeKeyCtl.CaptureScene
{
    public class SceneViewer
    {
        private List<Shape> _shapes = new();
        private List<UIElement> _uiElements = new();
        private List<TextBlock> _textBlocks = new();
        private Canvas _canvas;
        private MainWndState _mainWndState = MainWndState.Default;

        public SceneViewer(Canvas canvas)
        {
            var currentMainWindow = (MainWindow) Application.Current.MainWindow;
            currentMainWindow.MainWndStateChanged += CurrentMainWindowOnMainWndStateChanged;
            _canvas = canvas;
        }

        public void DrawScene(Scene scene)
        {
            if (_mainWndState != MainWndState.Default)
            {
                return;
            }
            
            var sceneSetting = scene.Setting;
            
            DrawScrapArea(sceneSetting.ScrapSetting, _canvas);
            DrawVirtualKey(sceneSetting.VirtualKeySettings, _canvas);
        }

        public void Discard()
        {
            foreach (var uiElement in _uiElements)
            {
                _canvas.Children.Remove(uiElement);
            }
            
            _uiElements.Clear();

            foreach (var shape in _shapes)
            {
                _canvas.Children.Remove(shape);
            }
            
            _shapes.Clear();

            foreach (var textBlock in _textBlocks)
            {
                _canvas.Children.Remove(textBlock);
            }
            
            _textBlocks.Clear();
        }

        private void CurrentMainWindowOnMainWndStateChanged(object? sender, MainWndState e)
        {
            if (e != MainWndState.Default)
            {
                Discard();
            }

            _mainWndState = e;
        }

        private void DrawScrapArea(ScrapSetting scrapSetting, Canvas canvas)
        {
            for (int i = _shapes.Count; i <= scrapSetting.ScrapInfos.Count; i++)
            {
                var rectShape = new Rectangle()
                {
                    Stroke = Brushes.Red,
                    Fill = Brushes.Transparent,
                    Focusable = false
                };
                
                canvas.Children.Add(rectShape);
                _shapes.Add(rectShape);
            }

            if (_shapes.Count > scrapSetting.ScrapInfos.Count)
            {
                var diff = _shapes.Count - scrapSetting.ScrapInfos.Count;
                for (int i = scrapSetting.ScrapInfos.Count; i < _shapes.Count; i++)
                {
                    var shape = _shapes[i];
                    canvas.Children.Remove(shape);
                }
                _shapes.RemoveRange(scrapSetting.ScrapInfos.Count, diff);
            }

            for (int i = 0; i < scrapSetting.ScrapInfos.Count; i++)
            {
                var shape = _shapes[i];
                var scrapArea = scrapSetting.ScrapInfos[i].ScrapArea;
                shape.Width = scrapArea.Width;
                shape.Height = scrapArea.Height;
            
                Canvas.SetLeft(shape, scrapArea.X);
                Canvas.SetTop(shape, scrapArea.Y);
            }
        }

        private void DrawVirtualKey(List<VirtualKeySetting> virtualKeySettings, Canvas canvas)
        {
            for (int i = _uiElements.Count; i < virtualKeySettings.Count; i++)
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
                var textBlock = new TextBlock()
                {
                    Width = 25,
                    FontSize = 13,
                    Foreground = (SolidColorBrush) brushConverter.ConvertFromString("#e9eaea"),
                    Background = (SolidColorBrush) brushConverter.ConvertFromString("Transparent"),
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    Focusable = false,
                };
                
                element.Children.Add(textBlock);
                
                canvas.Children.Add(element);
                _uiElements.Add(element);
                _textBlocks.Add(textBlock);
            }
            
            if (_uiElements.Count > virtualKeySettings.Count)
            {
                var diff = _uiElements.Count - virtualKeySettings.Count;
                for (int i = virtualKeySettings.Count; i < _uiElements.Count; i++)
                {
                    canvas.Children.Remove(_uiElements[i]);
                }
                _uiElements.RemoveRange(virtualKeySettings.Count, diff);
                _textBlocks.RemoveRange(virtualKeySettings.Count, diff);
            }

            for (int i = 0; i < virtualKeySettings.Count; i++)
            {
                var setting = virtualKeySettings[i];
                var left = setting.PressPos.X - 10;
                var top = setting.PressPos.Y - 10;
                
                _textBlocks[i].Text = $"{setting.BindKey.ToString()}";

                var uiElement = _uiElements[i];

                Canvas.SetLeft(uiElement, left);
                Canvas.SetTop(uiElement, top);
            }
        }
    }
}