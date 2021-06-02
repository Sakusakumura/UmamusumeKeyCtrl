using System;
using System.Drawing;
using System.Windows.Input;
using umamusumeKeyCtl.Helpers;
using umamusumeKeyCtl.Util;

namespace umamusumeKeyCtl.CaptureScene
{
    public class VirtualKey
    {
        private VirtualKeySetting _setting;
        public VirtualKeySetting Setting => _setting;
        public IntPtr UmaWndH { get; set; }

        public VirtualKey(VirtualKeySetting setting)
        {
            _setting = setting;
            UmaWndH = IntPtr.Zero;
        }

        public void Push()
        {
            if (UmaWndH == IntPtr.Zero)
            {
                return;
            }
            
            var prePos = MouseHelper.GetMousePosition();
            var rect = WindowHelper.GetWindowRect(UmaWndH);
            
            if (rect == Rectangle.Empty)
            {
                return;
            }

            Push(_setting.PressPos.ToSystemDrawingPoint());
            
            VirtualMouse.MoveTo(prePos);
        }

        private void Push(Point point)
        {
            var prePos = MouseHelper.GetMousePosition();

            VirtualMouse.MoveTo(point);
            VirtualMouse.Click(MouseButton.Left);
            VirtualMouse.MoveTo(prePos);
        }
    }
}