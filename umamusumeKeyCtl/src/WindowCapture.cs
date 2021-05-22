using System;
using System.Diagnostics;
using System.Drawing;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using WpfScreenHelper;

namespace umamusumeKeyCtl
{
    /// <summary>
    /// Capture window.
    /// </summary>
    public class WindowCapture : IDisposable
    {
        private int _captureInterval;
        private CancellationTokenSource _cancellationTokenSource;
        private string _captureWndName;
        private Task internalAsyncCaptureTask;

        private Subject<Bitmap> captureResultSubject;
        public IObservable<Bitmap> CaptureResultObservable => captureResultSubject;

        /// <summary>
        /// Constructor of Window Capture.
        /// Capturing will automatically start.
        /// </summary>
        /// <param name="captureWindowName"></param>
        /// <param name="captureInterval">Capture interval setting. (milliseconds)</param>
        public WindowCapture(string captureWindowName, int captureInterval)
        {
            this._captureInterval = captureInterval;
            this._captureWndName = captureWindowName;
            _cancellationTokenSource = new CancellationTokenSource();
            captureResultSubject = new Subject<Bitmap>();

            var targetHwnd = WindowHelper.GetHWndByName(captureWindowName);

            internalAsyncCaptureTask = InternalAsyncCapture(targetHwnd, _captureInterval, _cancellationTokenSource);
        }

        /// <summary>
        /// Stop capturing.
        /// </summary>
        public void StopCapture()
        {
            _cancellationTokenSource.Cancel();
            captureResultSubject.Dispose();
        }

        private async Task InternalAsyncCapture(IntPtr hWnd, int captureInterval, CancellationTokenSource cancellationTokenSource)
        {
            if (hWnd == IntPtr.Zero)
            {
                return;
            }
            
            while (cancellationTokenSource.Token.IsCancellationRequested == false)
            {
                using (Bitmap bitmap = PrintBitmap(hWnd))
                {
                    captureResultSubject.OnNext(bitmap ?? new Bitmap(1, 1));
                }

                await Task.Delay(captureInterval);
            }
            
            this.Dispose();
        }

        private Bitmap PrintBitmap(IntPtr hwnd)
        {
            var rectangle = WindowHelper.GetWindowRect(hwnd);

            var bitmap = TakeCopyOfScreen(rectangle);
            bitmap = bitmap.PerformScaling(Properties.Settings.Default.ImageResolutionWidth);
            
            return bitmap;
        }

        private static Bitmap TakeCopyOfScreen(RECT targetWndRect)
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
            while (internalAsyncCaptureTask.Status == TaskStatus.Running)
            {
                await Task.Yield();
            }
            
            _cancellationTokenSource.Dispose();
            captureResultSubject?.Dispose();
        }
    }
}