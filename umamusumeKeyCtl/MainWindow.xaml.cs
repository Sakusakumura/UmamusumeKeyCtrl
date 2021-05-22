using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace umamusumeKeyCtl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {   
            SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            
            InitializeComponent();
            
            var vm = new MainWndVM();
            
            var windowCapture = new WindowCapture(Properties.Settings.Default.CaptureWindowTitle, Properties.Settings.Default.CaptureInterval);

            windowCapture.CaptureResultObservable.Subscribe(bitmap => vm.OnPrintWnd(bitmap), exception => Console.Write(exception));

            Closing += (sender, args) => windowCapture.StopCapture();

            this.DataContext = vm;
        }
        
        [DllImport("user32.dll")]
        extern static IntPtr GetWindowDC(IntPtr hwnd);
        [DllImport("gdi32.dll")]
        extern static int GetDeviceCaps(IntPtr hdc, int index);
        [DllImport("user32.dll")]
        extern static int ReleaseDC(IntPtr hwnd, IntPtr hdc);
        
        [DllImport("User32.dll")]
        public static extern DPI_AWARENESS_CONTEXT SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT dpiContext);

        public enum DPI_AWARENESS_CONTEXT
        {
            DPI_AWARENESS_CONTEXT_DEFAULT = 0,
            DPI_AWARENESS_CONTEXT_UNAWARE = -1,
            DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = -2, 
            DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = -3,
            DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4,
            DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED = -5
        }
    }
}