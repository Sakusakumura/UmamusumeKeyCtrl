using System;
using System.Drawing;
using System.Windows;
using Point = System.Windows.Point;

namespace umamusumeKeyCtl.Helpers
{
    public class RectangleHelper
    {
        public static Rect GetRect(Point point1, Point point2)
        {
            var minX = (int) Math.Min(point1.X, point2.X);
            var maxX = (int) Math.Max(point1.X, point2.X);
            var minY = (int) Math.Min(point1.Y, point2.Y);
            var maxY = (int) Math.Max(point1.Y, point2.Y);
            
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}