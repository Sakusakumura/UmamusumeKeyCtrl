using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using umamusumeKeyCtl.Properties;

namespace umamusumeKeyCtl
{
    /// <summary>
    /// Implement extended methods related to bitmaps.
    /// These methods don't call sourceBitmap.Dispose()
    /// </summary>
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
            Bitmap cropped;
            
            var x = Math.Clamp(rectangle.X, 0, bitmap.Width - 1);
            var y = Math.Clamp(rectangle.Y, 0, bitmap.Height - 1);
            var width = Math.Clamp(rectangle.Width, 1, bitmap.Width - x);
            var height = Math.Clamp(rectangle.Height, 1, bitmap.Height - y);

            var clamped = new Rectangle(x, y, width, height);

            cropped = bitmap.Clone(clamped, bitmap.PixelFormat);
            
            return cropped;
        }

        /// <summary>
        /// Scale given bitmap to target width. Keeps aspect ratio.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetWidth"></param>
        /// <returns></returns>
        public static Bitmap PerformScaleByTransform(this Bitmap source, int targetWidth)
        {
            if (targetWidth <= 0)
            {
                throw new ArgumentException("targetWidth must be larger than 0.");
            }
            
            var targetSizing = (float) targetWidth / (float) source.Width;
                
            try
            {
                var scaled = new Bitmap(targetWidth, (int) (Settings.Default.GameAspectRatio * (float) targetWidth));
                using (Graphics graphics = Graphics.FromImage(scaled))
                {
                    graphics.ScaleTransform(targetSizing, targetSizing);
                    graphics.InterpolationMode = InterpolationMode.Bicubic;
                    graphics.DrawImage(source, new Rectangle(Point.Empty, source.Size));
                    graphics.Save();

                    return scaled;
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                throw;
            }
        }

        public static Bitmap PerformScale(this Bitmap source, int targetWidth)
        {
            var bmp = new Bitmap(targetWidth, (int) (Settings.Default.GameAspectRatio * (float) targetWidth));
            using (var g = Graphics.FromImage(bmp))
            {
                var sizing = (float)targetWidth / source.Width;
                if (sizing > 0.25)
                {
                    g.InterpolationMode = InterpolationMode.Bicubic;
                }
                else if (sizing > 0.5)
                {
                    g.InterpolationMode = InterpolationMode.Bilinear;
                }
                else
                {
                    g.InterpolationMode = InterpolationMode.Default;
                }
                
                g.DrawImage(source, new Rectangle(Point.Empty, source.Size));
                g.Save();
            }

            return bmp;
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
            
            try
            {
                using Graphics g = Graphics.FromImage(temp);
                ColorMatrix colorMatrix = new ColorMatrix(
                    new float[][]
                    {
                        new[] {.3f, .3f, .3f, 0, 0},
                        new[] {.59f, .59f, .59f, 0, 0},
                        new[] {.11f, .11f, .11f, 0, 0},
                        new[] {0f, 0, 0, 1, 0},
                        new[] {0f, 0, 0, 0, 1}
                    });

                using ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

                g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                throw;
            }

            return temp;
        }
        
        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            try
            {
                var bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, bitmap.PixelFormat);

                var bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height,
                    bitmap.HorizontalResolution, bitmap.VerticalResolution,
                    PixelFormats.Bgra32, null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                bitmap.UnlockBits(bitmapData);

                return bitmapSource;
            }
            catch (Exception e)
            {
                Debug.Write(e);
                throw;
            }
        }
    }
}