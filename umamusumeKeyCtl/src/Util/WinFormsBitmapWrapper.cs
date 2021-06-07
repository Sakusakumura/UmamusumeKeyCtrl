using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace umamusumeKeyCtl.Util
{
    internal class WinFormsBitmapWrapper : System.Windows.Media.Imaging.BitmapSource
    {
        //// based on:
        //// https://stackoverflow.com/a/32841840

        private readonly System.Drawing.Bitmap winFormsBitmap;

        public WinFormsBitmapWrapper( System.Drawing.Bitmap bitmap )
        {
            this.winFormsBitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
        }

        private static System.Windows.Media.PixelFormat ConvertPixelFormat( System.Drawing.Imaging.PixelFormat sourceFormat )
        {
            switch( sourceFormat )
            {
            case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                return PixelFormats.Bgr24;

            case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                return PixelFormats.Bgr32;

            case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                return PixelFormats.Bgra32;

            case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
                return PixelFormats.Pbgra32;

            default:
                throw new ArgumentException($"Pixel format conversion not implemented for {sourceFormat}!");
            }
        }

        #region BitmapSource

        public override double DpiX => this.winFormsBitmap.HorizontalResolution;

        public override double DpiY => this.winFormsBitmap.VerticalResolution;

        public override int PixelWidth => this.winFormsBitmap.Width;

        public override int PixelHeight => this.winFormsBitmap.Height;

        public override System.Windows.Media.PixelFormat Format => ConvertPixelFormat(this.winFormsBitmap.PixelFormat);

        public override BitmapPalette Palette => null;

        public override void CopyPixels( Int32Rect sourceRect, Array pixels, int stride, int offset )
        {
            var sourceData = this.winFormsBitmap.LockBits(
                new System.Drawing.Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                this.winFormsBitmap.PixelFormat);

            try
            {
                var length = sourceData.Stride * sourceData.Height;
                if( pixels is byte[] )
                {
                    var bytes = pixels as byte[];
                    System.Runtime.InteropServices.Marshal.Copy(sourceData.Scan0, bytes, 0, length);
                }
            }
            finally
            {
                this.winFormsBitmap.UnlockBits(sourceData);
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return (Freezable)new WinFormsBitmapWrapper(new System.Drawing.Bitmap(10, 10));
        }

#pragma warning disable 0067 // disable CS0067: The event is never used.
        public override event EventHandler<ExceptionEventArgs> DecodeFailed; // exception if not overridden
#pragma warning restore 0067

        #endregion
    }
}