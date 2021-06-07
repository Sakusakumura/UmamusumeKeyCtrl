using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using WpfScreenHelper;
using Point = System.Drawing.Point;

namespace umamusumeKeyCtl
{
    public class WindowHelper
    {
        [DllImport("user32.dll")]
        extern static bool SetProcessDPIAware();
        
        [DllImport("dwmapi.dll")]
        private static extern long DwmGetWindowAttribute(IntPtr hWnd, DWMWINDOWATTRIBUTE dwAttribute, out RECT rect, int cbAttribute);
        
        [DllImport("user32")]
        public static extern int GetClientRect(IntPtr hWnd, out Rect rect);
        
        [DllImport("user32")]
        public static extern int ClientToScreen(IntPtr hWnd, out System.Drawing.Point lpPoint);
        
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int width;
            public int height;

            public Rect(int left, int top, int width, int height)
            {
                this.left = left;
                this.top = top;
                this.width = width;
                this.height = height;
            }
        }
        
        enum DWMWINDOWATTRIBUTE
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,//ウィンドウのRect
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        };
        
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117,

            // http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
        }
        
        /// <summary>
        /// Get window handle of window that matches given name.
        /// </summary>
        /// <param name="wName">Window name.</param>
        /// <returns>Should contain the handle but may be zero if the title doesn't match</returns>
        public static IntPtr GetHWndByName(string wName)
        {
            IntPtr hWnd = IntPtr.Zero;
            foreach (Process pList in Process.GetProcesses())
            {
                if (pList.MainWindowTitle == wName)
                {
                    hWnd = pList.MainWindowHandle;
                }
            }
            return hWnd;
        }

        /// <summary>
        /// Get window handle of window that matches given name.
        /// </summary>
        /// <param name="wName">Window name.</param>
        /// <returns>Should contain the handle but may be zero if the title doesn't match</returns>
        public static async Task<IntPtr> AsyncGetHWndByName(string wName)
        {
            try
            {
                return await Task<IntPtr>.Run(() => { return GetHWndByName(wName); });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        public static Rectangle GetWindowRect(IntPtr hWnd)
        {
            var clientToScreenResult = ClientToScreen(hWnd, out Point point);

            var clientRectResult = GetClientRect(hWnd, out Rect rect);
            
            if (clientToScreenResult * clientRectResult == 0 || new Point(rect.width, rect.height) == Point.Empty)
            {
                return Rectangle.Empty;
            }

            Rectangle _Rectangle = new Rectangle(point.X, point.Y, rect.width, rect.height);

            return _Rectangle;
        }

        private static (double X, double Y) GetScreenScale(IntPtr targetHwnd)
        {
            var hwndSource = HwndSource.FromHwnd(targetHwnd);

            if (hwndSource == null)
            {
                return (1, 1);
            }
            
            return (hwndSource.CompositionTarget.TransformToDevice.M11, hwndSource.CompositionTarget.TransformToDevice.M12);
        }
    }
}