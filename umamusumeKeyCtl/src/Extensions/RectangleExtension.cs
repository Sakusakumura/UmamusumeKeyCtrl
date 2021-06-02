using System;
using System.Drawing;
using System.Windows;
using Point = System.Drawing.Point;

namespace umamusumeKeyCtl
{
    public static class RectangleExtension
    {
        public static Rectangle ToRectangle(this Rect rect)
        {
            return new((int) rect.X, (int) rect.Y, (int) rect.Width, (int) rect.Height);
        }
    }

    public static class PointExtension
    {
        public static System.Drawing.Point ToSystemDrawingPoint(this System.Windows.Point point)
        {
            return new ((int) point.X, (int) point.Y);
        }
    }
}