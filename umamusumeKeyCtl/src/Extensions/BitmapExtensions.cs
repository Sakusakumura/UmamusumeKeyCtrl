using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using umamusumeKeyCtl.Properties;

namespace umamusumeKeyCtl
{
    public static class BitmapExtensions
    {
        /// <summary>
        /// Crop bitmap to be size of given rectangle.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        public static Bitmap PerformCrop(this Bitmap bitmap, Rectangle rectangle)
        {
            using (bitmap)
            {
                Bitmap cropped;
            
                var x = Math.Clamp(rectangle.X, 0, bitmap.Width - 1);
                var y = Math.Clamp(rectangle.Y, 0, bitmap.Height - 1);
                var width = Math.Clamp(rectangle.Width, 1, bitmap.Width - x);
                var height = Math.Clamp(rectangle.Height, 1, bitmap.Height - y);

                var clamped = new Rectangle(x, y, width, height);

                cropped = bitmap.Clone(clamped, bitmap.PixelFormat);
            
                return cropped;
            }
        }

        /// <summary>
        /// Scale given bitmap to target width. Keeps aspect ratio.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetWidth"></param>
        /// <returns></returns>
        public static Bitmap PerformScale(this Bitmap source, int targetWidth)
        {
            if (targetWidth <= 0)
            {
                throw new ArgumentException("targetWidth must be larger than 0.");
            }
            
            using (source)
            {
                using (var cloned = (Bitmap) source.Clone())
                {
                    var targetSizing = (float) targetWidth / (float) cloned.Width;
                
                    try
                    {
                        var scaled = new Bitmap(targetWidth,
                            (int) (Settings.Default.GameAspectRatio * (float) targetWidth));
                        using (Graphics graphics = Graphics.FromImage(scaled))
                        {
                            graphics.ScaleTransform(targetSizing, targetSizing);
                            graphics.InterpolationMode = InterpolationMode.Bicubic;
                            graphics.DrawImage(cloned, new Rectangle(Point.Empty, cloned.Size));

                            return scaled;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Print(e.ToString());
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Make source bitmap to grayscale.
        ///
        /// Copy from https://web.archive.org/web/20130208001434/http://tech.pro:80/tutorial/660/csharp-tutorial-convert-a-color-image-to-grayscale
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Bitmap PerformGrayScale(this Bitmap source)
        {
            Bitmap temp = new Bitmap(source.Width, source.Height);
            
            using (source)
            {
                try
                {
                    Graphics g = Graphics.FromImage(temp);
                    ColorMatrix colorMatrix = new ColorMatrix(
                        new float[][]
                        {
                            new[] {.3f, .3f, .3f, 0, 0},
                            new[] {.59f, .59f, .59f, 0, 0},
                            new[] {.11f, .11f, .11f, 0, 0},
                            new[] {0f, 0, 0, 1, 0},
                            new[] {0f, 0, 0, 0, 1}
                        });

                    using (ImageAttributes attributes = new ImageAttributes())
                    {
                        attributes.SetColorMatrix(colorMatrix);

                        g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
                    }
                    
                    g.Dispose();
                }
                catch (Exception e)
                {
                    Debug.Print(e.ToString());
                    throw;
                }
            }

            return temp;
        }
    }
}