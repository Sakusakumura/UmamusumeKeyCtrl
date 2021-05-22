using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using WpfScreenHelper;

namespace umamusumeKeyCtl
{
    public class WindowHelper
    {
        [DllImport("user32.dll")]
        extern static bool SetProcessDPIAware();
        
        [DllImport("dwmapi.dll")]
        private static extern long DwmGetWindowAttribute(IntPtr hWnd, DWMWINDOWATTRIBUTE dwAttribute, out RECT rect, int cbAttribute);
        
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

        public static RECT GetWindowRect(IntPtr hWnd)
        {
            var factor = getScalingFactor(hWnd);
            
            RECT rect;
            
            DwmGetWindowAttribute(hWnd,
                DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
                out rect,
                Marshal.SizeOf(typeof(RECT)));

            return rect;
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

        private static float getScalingFactor(IntPtr hwnd)
        {
            Graphics g = Graphics.FromHwnd(hwnd);
            IntPtr desktop = g.GetHdc();
            int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES); 

            float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;

            return ScreenScalingFactor; // 1.25 = 125%
        }
    }
}