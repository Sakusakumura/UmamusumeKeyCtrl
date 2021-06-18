using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using umamusumeKeyCtl.Helpers;
using umamusumeKeyCtl.Properties;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;

namespace umamusumeKeyCtl.CaptureScene
{
    public class ScrapSettingModifier
    {
        private SelectedPoint _selectedPoint = SelectedPoint.LeftCenter;
        private EditState _editState = EditState.Waiting;
        private List<Shape> _elements = new ();
        private ScrapSetting _scrapSetting;
        
        private Point _firstPos;
        private Rect _tempRect;
        private int _targetIndex;
        private Canvas _canvas;
        private ScrapSettingMaker _maker;

        public event Action<ScrapSetting> ChangeScrapSetting;
        
        public ScrapSettingModifier(ScrapSetting scrapSetting, Canvas drawToCanvas)
        {
            _scrapSetting = scrapSetting;
            _canvas = drawToCanvas;

            foreach (var scrapInfo in scrapSetting.ScrapInfos)
            {
                DrawScrapInfo(scrapInfo, _canvas);
            }
        }

        public void Repaint()
        {
            Discard();

            foreach (var scrapInfo in _scrapSetting.ScrapInfos)
            {
                DrawScrapInfo(scrapInfo, _canvas);
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
                if (_editState == EditState.Adding)
                {
                    _maker.Cancel();
                    _maker.MadeScrapSetting -= OnSettingCreatedNewly;
                    _maker = null;
                }
                _editState = EditState.Waiting;
                return;
            }
            if (mode == EditMode.Add)
            {
                _maker = new ScrapSettingMaker(_canvas, _canvas, true);
                _maker.MadeScrapSetting += OnSettingCreatedNewly;

                _editState = EditState.Adding;
                return;
            }
            if (mode == EditMode.Remove)
            {
                if (_editState == EditState.Adding)
                {
                    _maker.Cancel();
                    _maker.MadeScrapSetting -= OnSettingCreatedNewly;
                    _maker = null;
                }
                _editState = EditState.Removing;
                return;
            }
        }
        
        private void DrawScrapInfo(ScrapInfo scrapInfo, Canvas drawToCanvas)
        {
            var rectShape = new Rectangle()
            {
                Stroke = Brushes.Red,
                Fill = Brushes.Transparent,
                Focusable = true
            };
            
            rectShape.MouseLeftButtonDown += (_, args) =>
            {
                OnMouseLeftDown(drawToCanvas, args, scrapInfo);
            };
            rectShape.MouseLeftButtonUp += (_, _) => OnRemoveSettingSelected(scrapInfo);
            drawToCanvas.MouseLeftButtonUp += (sender, args) => OnMouseLeftButtonUp((Canvas) sender, rectShape, args);
            drawToCanvas.MouseMove += (o, args) => OnMouseMove((Canvas) o, rectShape, args, scrapInfo);

            drawToCanvas.Children.Add(rectShape);
            _elements.Add(rectShape);

            rectShape.Width = scrapInfo.ScrapArea.Width;
            rectShape.Height = scrapInfo.ScrapArea.Height;
            
            Canvas.SetLeft(rectShape, scrapInfo.ScrapArea.X);
            Canvas.SetTop(rectShape, scrapInfo.ScrapArea.Y);
        }
        
        private void OnSettingCreatedNewly(ScrapSetting settings)
        {
            if (settings == null || settings.ScrapInfos.Count == 0)
            {
                return;
            }

            var copy = settings.ScrapInfos.ToArray();

            // remove elements if exists in _scrapSetting.Scrapinfos.
            foreach (var scrapInfo in copy)
            {
                var indexed = new ScrapInfo(GetNewIndex(_scrapSetting.ScrapInfos), scrapInfo.ScrapArea);
                _scrapSetting.ScrapInfos.Add(indexed);
            }
            
            ChangeScrapSetting?.Invoke(_scrapSetting);

            _editState = EditState.Waiting;
        }

        private int GetNewIndex(List<ScrapInfo> infos)
        {
            int temp;
            
            if (infos.Count == 0)
            {
                return 0;
            }
            
            int maxIndex = infos.Select(val => val.Index).Max();

            for (temp = 0; temp <= maxIndex + 1; temp++)
            {
                if (infos.Any(val => val.Index == temp))
                {
                    continue;
                }
                
                break;
            }

            return temp;
        }

        private void OnRemoveSettingSelected(ScrapInfo setting)
        {
            if (_editState != EditState.Removing)
            {
                return;
            }

            // Find and remove target scrapInfo from list in order to edit it's settings.
            var target = _scrapSetting.ScrapInfos.Find(val => val.Index == setting.Index);
            _scrapSetting.ScrapInfos.Remove(target);
            
            ChangeScrapSetting?.Invoke(_scrapSetting);
        }
        
        private void OnMouseLeftDown(Canvas sender, MouseButtonEventArgs args, ScrapInfo info)
        {
            if (_editState != EditState.Waiting)
            {
                return;
            }

            foreach (var element in _elements)
            {
                element.Focusable = false;
            }

            _firstPos = args.GetPosition(sender);

            _targetIndex = info.Index;
            _selectedPoint = CalcurateSelectedPoint(info.ScrapArea, args.GetPosition(sender));
            _editState = EditState.Moving;

            _tempRect = info.ScrapArea;
        }

        private void OnMouseMove(Canvas sender, Shape shape, MouseEventArgs args, ScrapInfo info)
        {
            if (_editState != EditState.Moving || info.Index != _targetIndex)
            {
                return;
            }
            
            var relVec = GetRelPos(_firstPos, args.GetPosition(sender));

            _tempRect = GetRect(_selectedPoint, info, relVec);

            Canvas.SetTop(shape, _tempRect.Top);
            Canvas.SetLeft(shape, _tempRect.Left);
            shape.Width = _tempRect.Width;
            shape.Height = _tempRect.Height;
        }

        private Rect GetRect(SelectedPoint selectedPoint, ScrapInfo info, Point relVec)
        {
            if (selectedPoint == SelectedPoint.LeftCenter)
            {
                return RectangleHelper.GetRect(info.ScrapArea.TopRight, Point.Add(info.ScrapArea.BottomLeft, new Vector(relVec.X, 0.0)));
            }
            if (selectedPoint == SelectedPoint.LeftTop)
            {
                return RectangleHelper.GetRect(info.ScrapArea.TopRight, Point.Add(info.ScrapArea.BottomLeft, new Vector(relVec.X, relVec.Y)));
            }
            if (selectedPoint == SelectedPoint.CenterTop)
            {
                return RectangleHelper.GetRect(info.ScrapArea.TopRight, Point.Add(info.ScrapArea.BottomLeft, new Vector(0.0, relVec.Y)));
            }
            if (selectedPoint == SelectedPoint.RightTop)
            {
                return RectangleHelper.GetRect(info.ScrapArea.TopLeft, Point.Add(info.ScrapArea.BottomRight, new Vector(relVec.X, relVec.Y)));
            }
            if (selectedPoint == SelectedPoint.RightCenter)
            {
                return RectangleHelper.GetRect(info.ScrapArea.TopLeft, Point.Add(info.ScrapArea.BottomRight, new Vector(relVec.X, 0.0)));
            }
            if (selectedPoint == SelectedPoint.RightBottom)
            {
                return RectangleHelper.GetRect(info.ScrapArea.BottomLeft, Point.Add(info.ScrapArea.TopRight, new Vector(relVec.X, relVec.Y)));
            }
            if (selectedPoint == SelectedPoint.CenterBottom)
            {
                return RectangleHelper.GetRect(info.ScrapArea.BottomLeft, Point.Add(info.ScrapArea.TopRight, new Vector(0.0, relVec.Y)));
            }
            if (selectedPoint == SelectedPoint.LeftBottom)
            {
                return RectangleHelper.GetRect(info.ScrapArea.BottomRight, Point.Add(info.ScrapArea.TopLeft, new Vector(relVec.X, relVec.Y)));
            }
            if (selectedPoint == SelectedPoint.Center)
            {
                return RectangleHelper.GetRect(Point.Add(info.ScrapArea.BottomLeft, new Vector(relVec.X, relVec.Y)), Point.Add(info.ScrapArea.TopRight, new Vector(relVec.X, relVec.Y)));
            }
            
            return Rect.Empty;
        }

        private void OnMouseLeftButtonUp(Canvas sender, Shape shape, MouseButtonEventArgs args)
        {
            if (_editState != EditState.Moving)
            {
                return;
            }

            foreach (var element in _elements)
            {
                element.Focusable = true;
            }

            var target = _scrapSetting.ScrapInfos.Find(val => val.Index == _targetIndex);
            _scrapSetting.ScrapInfos.Remove(target);

            var newScrapInfo = new ScrapInfo(_targetIndex, GetClampedRect(_tempRect));
            
            _scrapSetting.ScrapInfos.Add(newScrapInfo);
            
            ChangeScrapSetting?.Invoke(_scrapSetting);
            
            Discard();
            foreach (var scrapInfo in _scrapSetting.ScrapInfos)
            {
                DrawScrapInfo(scrapInfo, sender);
            }

            _editState = EditState.Waiting;
        }

        private Rect GetClampedRect(Rect source)
        {
            double maxWidth = Settings.Default.ImageResolutionWidth;
            double maxHeight = maxWidth * Settings.Default.GameAspectRatio;
            double minExtent = Settings.Default.MinExtent;
            var x = Math.Clamp(source.X, 0, maxWidth - minExtent);
            var y = Math.Clamp(source.Y, 0, maxHeight - minExtent);
            var width = Math.Clamp(source.Width, minExtent, maxWidth - x);
            var height = Math.Clamp(source.Height, minExtent, maxHeight - y);

            return new(x, y, width, height);
        }

        private Point GetRelPos(Point from, Point to)
        {
            return new (to.X - from.X, to.Y - from.Y);
        }

        private SelectedPoint CalcurateSelectedPoint(Rect area, Point point)
        {
            if (point.X < area.X + area.Width * CornerCenterRatio.BottomCorner)
            {
                if (point.Y < area.Y + area.Height * CornerCenterRatio.BottomCorner)
                {
                    return SelectedPoint.LeftBottom;
                }

                if (point.Y < area.Y + area.Height * CornerCenterRatio.Center)
                {
                    return SelectedPoint.LeftCenter;
                }

                return SelectedPoint.LeftTop;
            }
            
            if (point.X < area.X + area.Width * CornerCenterRatio.Center)
            {
                if (point.Y < area.Y + area.Height * CornerCenterRatio.BottomCorner)
                {
                    return SelectedPoint.CenterBottom;
                }

                if (point.Y < area.Y + area.Height * CornerCenterRatio.Center)
                {
                    return SelectedPoint.Center;
                }

                return SelectedPoint.CenterTop;
            }
            
            if (point.Y < area.Y + area.Height * CornerCenterRatio.BottomCorner)
            {
                return SelectedPoint.RightBottom;
            }

            if (point.Y < area.Y + area.Height * CornerCenterRatio.Center)
            {
                return SelectedPoint.RightCenter;
            }

            return SelectedPoint.RightTop;
        }

        private class CornerCenterRatio
        {
            public static double CornerRatio = 0.3;

            public static double BottomCorner = CornerRatio;
            public static double Center = 1.0 - CornerRatio;
            public static double TopCorner = 1.0;
        }

        private enum EditState
        {
            Waiting,
            Removing,
            Adding,
            Moving
        }

        private enum SelectedPoint
        {
            LeftCenter,
            LeftTop,
            CenterTop,
            RightTop,
            RightCenter,
            RightBottom,
            CenterBottom,
            LeftBottom,
            Center
        }
    }
}