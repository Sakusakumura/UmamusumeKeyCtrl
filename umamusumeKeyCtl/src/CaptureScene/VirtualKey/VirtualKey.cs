using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using umamusumeKeyCtl.Helpers;
using umamusumeKeyCtl.Properties;
using umamusumeKeyCtl.UserInput;

namespace umamusumeKeyCtl.CaptureScene
{
    public class VirtualKey
    {
        private VirtualKeySetting _setting;
        public VirtualKeySetting Setting => _setting;
        public IntPtr UmaWndH { get; set; }

        private object _isKeyPressedBeforeLock = new object();
        private bool _isKeyPressedBefore = false;

        private bool IsKeyPressedBefore
        {
            get => _isKeyPressedBefore;
            set
            {
                lock (_isKeyPressedBeforeLock)
                {
                    _isKeyPressedBefore = value;
                }
            }
        }
        
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        public VirtualKey(VirtualKeySetting setting)
        {
            _setting = setting;
            UmaWndH = IntPtr.Zero;
        }

        public void Initialize()
        {
            _isKeyPressedBefore = false;
        }

        public void Perform(KeyPressedArgs pressedArgs)
        {
            if (pressedArgs.WParam == WM_KEYUP)
            {
                Debug.Print($"[{this.GetType().Name}] Key: {Setting.BindKey.ToString()}, return condition: pressedArgs.WParam == WM_KEYUP");
                IsKeyPressedBefore = false;
                return;
            }

            if (UmaWndH == IntPtr.Zero || IsKeyPressedBefore || UmaWndH != WindowHelper.GetForegroundWindow())
            {
                return;
            }

            IsKeyPressedBefore = true;

            var rect = WindowHelper.GetWindowRect(UmaWndH);

            if (rect == Rectangle.Empty)
            {
                return;
            }

            var dest = rect.Location;
            var scaled = CalcurateScaledPoint(_setting.PressPos, rect);
            dest.Offset(scaled);

            VirtualKeyPushExecutor.Instance.EnQueue(dispatcher => Perform(dest, dispatcher));
        }

        /// <summary>
        /// サンプル画像のサイズが横幅300px(デフォルトで)になるように縮小しているので、戻す必要がある
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Point CalcurateScaledPoint(System.Windows.Point source, Rectangle windowRectangle)
        {
            double k = windowRectangle.Width / (double) Settings.Default.ImageResolutionWidth;
            
            return new Point((int) (source.X * k), (int) (source.Y * k));
        }

        private Task Perform(Point point, Dispatcher dispatcher)
        {
            var random = new Random().Next();
            Debug.Print($"[{this.GetType().Name}] Start. ({random})");

            try
            {
                var prePos = MouseHelper.GetMousePosition();

                dispatcher.Invoke(() =>
                {
                    VirtualMouse.MoveTo(point);
                    VirtualMouse.Down(MouseButton.Left);
                });
                
                Thread.Sleep(20);

                dispatcher.Invoke(() =>
                {
                    VirtualMouse.Up(MouseButton.Left);
                });

                Thread.Sleep(45);
                
                dispatcher.Invoke(() =>
                {
                    VirtualMouse.MoveTo(prePos);
                });
            }
            catch (Exception e)
            {
                Debug.Print($"[{this.GetType().Name}] Exception. ({random})");
                Debug.WriteLine(e);
                throw;
            }
            
            Debug.Print($"[{this.GetType().Name}] End. ({random})");
            
            return Task.CompletedTask;
        }
    }
}