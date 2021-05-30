using System;
using System.Drawing;

namespace umamusumeKeyCtl
{
    public class CaptureResult : IDisposable
    {
        public bool IsSucceed { get; set; }
        public Bitmap Image { get; set; }

        public CaptureResult()
        {
        }

        public CaptureResult(bool isSucceed, Bitmap image)
        {
            IsSucceed = isSucceed;
            Image = image;
        }

        public void Dispose()
        {
            Image?.Dispose();
        }
    }
}