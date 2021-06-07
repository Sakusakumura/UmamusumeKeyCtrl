using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
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
        
        private bool _isKeyPressedBefore = false;
        
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        public VirtualKey(VirtualKeySetting setting)
        {
            _setting = setting;
            UmaWndH = IntPtr.Zero;
        }

        public void OnKeyHook(KeyPressedArgs pressedArgs)
        {
            if (pressedArgs.WParam == WM_KEYUP)
            {
                _isKeyPressedBefore = false;
                return;
            }

            if (UmaWndH == IntPtr.Zero || _isKeyPressedBefore)
            {
                return;
            }

            _isKeyPressedBefore = true;
            
            var prePos = MouseHelper.GetMousePosition();
            var rect = WindowHelper.GetWindowRect(UmaWndH);
            
            if (rect == Rectangle.Empty)
            {
                return;
            }

            var dest = rect.Location;
            var scaled = CalcurateScaledPoint(_setting.PressPos, rect);
            dest.Offset(scaled);

            VirtualKeyPushExecutor.Instance.EnQueue(OnKeyHook(dest));
            
            VirtualMouse.MoveTo(prePos);
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

        private Task OnKeyHook(Point point)
        {
            var prePos = MouseHelper.GetMousePosition();

            VirtualMouse.MoveTo(point);
            Thread.Sleep(40);
            VirtualMouse.Down(MouseButton.Left);
            Thread.Sleep(40);
            VirtualMouse.Up(MouseButton.Left);
            Thread.Sleep(40);
            VirtualMouse.MoveTo(prePos);

            return Task.CompletedTask;
        }
    }
}