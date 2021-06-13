namespace umamusumeKeyCtl
{
    public class CaptureSetting
    {
        public int Interval { get; set; }
        public string CaptureWndName { get; set; }

        public CaptureSetting(int interval, string captureWndName)
        {
            Interval = interval;
            CaptureWndName = captureWndName;
        }
    }
}