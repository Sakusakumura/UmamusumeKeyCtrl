using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Image = System.Drawing.Image;

namespace umamusumeKeyCtl
{
    /// <summary>
    /// Capture window.
    /// </summary>
    public class WindowCapture : IDisposable
    {
        private CaptureSetting _captureSetting;
        private CancellationTokenSource _cancellationTokenSource;
        private Task internalAsyncCaptureTask;

        private Subject<Bitmap> _captureResultSubject;
        public IObservable<Bitmap> CaptureResultObservable => _captureResultSubject;

        private Bitmap _waitingImage;

        /// <summary>
        /// Constructor of Window Capture.
        /// Capturing will automatically start.
        /// </summary>
        /// <param name="captureWindowName"></param>
        /// <param name="captureInterval">Capture interval setting. (milliseconds)</param>
        public WindowCapture(CaptureSetting setting)
        {
            _captureSetting = setting;
            _cancellationTokenSource = new CancellationTokenSource();
            _captureResultSubject = new Subject<Bitmap>();

            using (var image = (Bitmap) Image.FromFile("Resources/devilman.bmp"))
            {
                _waitingImage = (Bitmap) image.Clone();
            }

            internalAsyncCaptureTask = InternalAsyncCapture(_captureSetting, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Stop capturing.
        /// </summary>
        public void StopCapture()
        {
            _cancellationTokenSource.Cancel();
            _captureResultSubject.Dispose();
        }

        private async Task InternalAsyncCapture(CaptureSetting captureSetting, CancellationToken token)
        {
            var hWnd = await WindowHelper.AsyncGetHWndByName(captureSetting.CaptureWndName);
            
            while (token.IsCancellationRequested == false)
            {
                if (hWnd == IntPtr.Zero)
                {
                    _captureResultSubject.OnNext((Bitmap) _waitingImage.Clone());
                }
                
                while (hWnd == IntPtr.Zero)
                {
                    await Task.Delay(1000);

                    hWnd = await WindowHelper.AsyncGetHWndByName(captureSetting.CaptureWndName);
                }

                using (CaptureResult result = await Task<CaptureResult>.Run(() => PrintBitmap(hWnd)))
                {
                    if (result.IsSucceed == false)
                    {
                        hWnd = IntPtr.Zero;
                    }

                    _captureResultSubject.OnNext((Bitmap) result.Image?.Clone() ?? new Bitmap(1, 1));
                }

                await Task.Delay(captureSetting.Interval);
            }
            
            this.Dispose();
        }

        private CaptureResult PrintBitmap(IntPtr hwnd)
        {
            var rectangle = WindowHelper.GetWindowRect(hwnd);

            if (rectangle == Rectangle.Empty)
            {
                return new CaptureResult(false, (Bitmap) _waitingImage.Clone());
            }

            var bitmap = TakeCopyOfScreen(rectangle);
            bitmap = bitmap.PerformScale(Properties.Settings.Default.ImageResolutionWidth);
            
            return new CaptureResult(true, bitmap);
        }

        private Bitmap TakeCopyOfScreen(Rectangle targetWndRect)
        {
            double screenLeft = targetWndRect.Left;
            double screenTop = targetWndRect.Top;
            double screenWidth = targetWndRect.Width;
            double screenHeight = targetWndRect.Height;

            Bitmap bmp = new Bitmap((int) screenWidth, (int) screenHeight);
            
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen((int)screenLeft, (int)screenTop, 0, 0, bmp.Size);
            }
            
            return bmp;
        }

        public async void Dispose()
        {
            while (internalAsyncCaptureTask?.Status == TaskStatus.Running)
            {
                await Task.Yield();
            }
            
            _cancellationTokenSource?.Dispose();
            _captureResultSubject?.Dispose();
            _waitingImage?.Dispose();
        }
    }
}