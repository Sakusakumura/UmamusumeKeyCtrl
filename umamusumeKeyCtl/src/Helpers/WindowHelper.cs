using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Point = System.Drawing.Point;

namespace umamusumeKeyCtl.Helpers
{
    public class WindowHelper
    {
        [DllImport("user32")]
        public static extern int GetClientRect(IntPtr hWnd, out Rect rect);
        
        [DllImport("user32")]
        public static extern int ClientToScreen(IntPtr hWnd, out Point lpPoint);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        
        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

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
    }
}