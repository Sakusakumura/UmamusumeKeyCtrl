using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

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
        public static Bitmap CropBitmap(this Bitmap bitmap, Rectangle rectangle)
        {
            Bitmap cropped = bitmap.Clone(rectangle, bitmap.PixelFormat);
            
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
    }
}