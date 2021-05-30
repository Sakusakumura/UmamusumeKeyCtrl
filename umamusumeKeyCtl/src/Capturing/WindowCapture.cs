using System;
using System.Drawing;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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

            _waitingImage = (Bitmap) Image.FromFile("devilman.jpg");

            internalAsyncCaptureTask = InternalAsyncCapture(_captureSetting, _cancellationTokenSource);
        }

        /// <summary>
        /// Stop capturing.
        /// </summary>
        public void StopCapture()
        {
            _cancellationTokenSource.Cancel();
            _captureResultSubject.Dispose();
        }

        private async Task InternalAsyncCapture(CaptureSetting captureSetting, CancellationTokenSource cancellationTokenSource)
        {
            var hWnd = await WindowHelper.AsyncGetHWndByName(captureSetting.CaptureWndName);
            
            while (cancellationTokenSource.Token.IsCancellationRequested == false)
            {
                while (hWnd == IntPtr.Zero)
                {
                    await Task.Run(() => { return Task.Delay(1000); });

                    hWnd = await WindowHelper.AsyncGetHWndByName(captureSetting.CaptureWndName);
                }

                using (CaptureResult result = await AsyncPrintBitmap(hWnd))
                {
                    if (result.IsSucceed == false)
                    {
                        hWnd = IntPtr.Zero;
                    }
                    _captureResultSubject.OnNext(result.Image ?? new Bitmap(1, 1));
                }

                await Task.Run(() => { return Task.Delay(captureSetting.Interval); });
            }
            
            this.Dispose();
        }

        private async Task<CaptureResult> AsyncPrintBitmap(IntPtr hwnd)
        {
            return await Task<CaptureResult>.Run(() => { return PrintBitmap(hwnd); });
        }

        private CaptureResult PrintBitmap(IntPtr hwnd)
        {
            var rectangle = WindowHelper.GetWindowRect(hwnd);

            if (rectangle == Rectangle.Empty)
            {
                return new CaptureResult(false, _waitingImage);
            }

            var bitmap = TakeCopyOfScreen(rectangle);
            bitmap = bitmap.PerformScaling(Properties.Settings.Default.ImageResolutionWidth);
            
            return new CaptureResult(true, bitmap);
        }

        private static Bitmap TakeCopyOfScreen(Rectangle targetWndRect)
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