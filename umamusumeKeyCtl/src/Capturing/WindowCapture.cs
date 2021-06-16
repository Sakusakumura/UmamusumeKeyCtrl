using System;
using System.Diagnostics;
using System.Drawing;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using umamusumeKeyCtl.Helpers;
using umamusumeKeyCtl.Properties;

namespace umamusumeKeyCtl
{
    /// <summary>
    /// Provides functions to capture a specific window asynchronously.
    /// </summary>
    public class WindowCapture : IDisposable
    {
        private CaptureSetting _captureSetting;
        private CancellationTokenSource _cancellationTokenSource;

        private Subject<Bitmap> _captureResultSubject;
        public IObservable<Bitmap> CaptureResultObservable => _captureResultSubject;

        private Bitmap _waitingImage;

        private bool _stopIfBackground;
        private bool _sendWaitingImage;

        /// <summary>
        /// Constructor of Window Capture.
        /// Capturing will automatically start.
        /// </summary>
        /// <param name="setting">Capture setting.</param>
        /// <param name="stopIfBackground">If true, capturing will be paused when target window is in the background.</param>
        public WindowCapture(CaptureSetting setting, bool stopIfBackground = true, bool sendWaitingImage = true)
        {
            _captureSetting = setting;
            _stopIfBackground = stopIfBackground;
            _sendWaitingImage = sendWaitingImage;
            _cancellationTokenSource = new CancellationTokenSource();
            _captureResultSubject = new Subject<Bitmap>();

            _waitingImage = CreateWaitingImage();

            _ = InternalAsyncCapture(_captureSetting, _cancellationTokenSource.Token);
        }

        private Bitmap CreateWaitingImage()
        {
            var width = Settings.Default.ImageResolutionWidth;
            var height = (int) (Settings.Default.GameAspectRatio * Settings.Default.ImageResolutionWidth);

            var image = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(image))
            {
                var color = ColorTranslator.FromHtml("#2A2C2B");
                graphics.Clear(color);
            }

            return image;
        }

        /// <summary>
        /// Capture target window asynchronosly.
        /// Capturing will be paused when target window is not foreground or closed.
        /// To shut down capturing, call StopCapture().
        /// </summary>
        /// <param name="captureSetting"></param>
        /// <param name="token"></param>
        private async Task InternalAsyncCapture(CaptureSetting captureSetting, CancellationToken token)
        {
            try
            {
                var hWnd = await WindowHelper.AsyncGetHWndByName(captureSetting.CaptureWndName);
                var stopWatch = new Stopwatch();

                // Send waiting image for initialize.
                if (_sendWaitingImage)
                { 
                    _captureResultSubject.OnNext((Bitmap) _waitingImage.Clone());
                }

                while (token.IsCancellationRequested == false)
                {
                    stopWatch.Reset();

                    if (hWnd == IntPtr.Zero)
                    {
                        _captureResultSubject.OnNext((Bitmap) _waitingImage.Clone());
                    }

                    // Wait when target window is not found or closed.
                    while (hWnd == IntPtr.Zero && token.IsCancellationRequested == false)
                    {
                        await Task.Delay(500);

                        hWnd = await WindowHelper.AsyncGetHWndByName(captureSetting.CaptureWndName);
                    }

                    // Wait when target window is in the background, not closed, and not canceled.
                    while (_stopIfBackground && hWnd != WindowHelper.GetForegroundWindow() && WindowHelper.IsWindow(hWnd) && token.IsCancellationRequested == false)
                    {
                        await Task.Delay(250);
                    }

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    // Generate a captured image.
                    stopWatch.Start();
                    using (CaptureResult result = await Task<CaptureResult>.Run(() => PrintBitmap(hWnd)))
                    {
                        if (result.IsSucceed == false)
                        {
                            hWnd = IntPtr.Zero;
                        }

                        _captureResultSubject.OnNext((Bitmap) result.Image?.Clone() ?? new Bitmap(1, 1));
                    }

                    stopWatch.Stop();

                    // The interval time will be shorter or longer due to processing time.
                    var waitTime = Math.Max(captureSetting.Interval - stopWatch.ElapsedMilliseconds, 0);

                    await Task.Delay((int) waitTime);
                }
            }
            catch (Exception e)
            {
                Debug.Write(e);
                throw;
            }
            finally
            {
                this.Dispose();
            }
        }

        private CaptureResult PrintBitmap(IntPtr hwnd)
        {
            var rectangle = WindowHelper.GetWindowRect(hwnd);

            if (rectangle == Rectangle.Empty)
            {
                return new CaptureResult(false, (Bitmap) _waitingImage.Clone());
            }

            using var copyOfScreen = TakeCopyOfScreen(rectangle);
            using var scaled = copyOfScreen.PerformScale(Settings.Default.ImageResolutionWidth);
            var grayscaled = scaled.PerformGrayScale();
            
            return new CaptureResult(true, grayscaled);
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

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _captureResultSubject?.Dispose();
            _waitingImage?.Dispose();
        }
    }
}