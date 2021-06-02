using System;
using System.Drawing;
using System.Drawing.Imaging;

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
        /// <param name="targetWidth">target image width</param>
        public static Bitmap PerformScaling(this Bitmap sourceBitmap, int targetWidth)
        {
            Image image = sourceBitmap;
            
            var targetSizing = (float) targetWidth / (float) sourceBitmap.Width;
            var targetHeight = (int) ((float) image.Height * targetSizing);

            Bitmap bitmap = new Bitmap(targetWidth, targetHeight);
            // サイズ変更した画像を作成する
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                // 変更サイズを取得する
                int widthToScale = (int)(image.Width * targetSizing);
                int heightToScale = (int)(image.Height * targetSizing);

                // 背景色を塗る
                SolidBrush solidBrush = new SolidBrush(Color.Black);
                graphics.FillRectangle(solidBrush, new RectangleF(0, 0, targetWidth, targetHeight));

                // サイズ変更した画像に、左上を起点に変更する画像を描画する
                graphics.DrawImage(image, 0, 0, widthToScale, heightToScale);

                return bitmap;
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
                    Console.WriteLine(e);
                    throw;
                }
            }

            return temp;
        }
    }
}