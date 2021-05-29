using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using umamusumeKeyCtl.UserInput;

namespace umamusumeKeyCtl.CaptureSettingSets.ImageScrapping
{
    public class ScrapSettingMaker
    {
        public event Action<ScrapSetting> OnMadeScrapSetting;

        private Canvas _canvas;
        private UIElement _element;
        private bool _drawRectangle;

        private List<RectangleGetter> _getters;
        private List<ScrapInfo> _rectangles;
        
        public ScrapSettingMaker(Canvas canvas, UIElement eventListenSource, bool drawRectangle)
        {
            _canvas = canvas;
            _element = eventListenSource;
            _drawRectangle = drawRectangle;
            _rectangles = new();
            _getters = new List<RectangleGetter>();
            
            var getter = new RectangleGetter(canvas, eventListenSource, drawRectangle);
            getter.OnGetRectangle += OnGetRectangle;
            
            _getters.Add(getter);
        }

        public void Cancel()
        {
            if (_drawRectangle)
            {
                foreach (var getter in _getters)
                {
                    getter.Unload();
                    getter.Cancel();
                }
            }
            
            _getters.Clear();
        }

        private void OnGetRectangle(Rect rectangle)
        {
            _rectangles.Add(new ScrapInfo(_rectangles.Count, rectangle));

            _getters.Last().OnGetRectangle -= OnGetRectangle;
            
            if (MessageBox.Show("続けて設定しますか？", "Question", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var getter = new RectangleGetter(_canvas, _element, _drawRectangle);
                getter.OnGetRectangle += OnGetRectangle;
                
                _getters.Add(getter);

                return;
            }

            if (_drawRectangle)
            {
                foreach (var getter in _getters)
                {
                    getter.Unload();
                }
            }
            
            _getters.Clear();
            
            OnMadeScrapSetting?.Invoke(new ScrapSetting(_rectangles));
        }
    }
}