namespace umamusumeKeyCtl.CaptureSettingSets.VirtualKeyPushing
{
    public class VirtualKey
    {
        private VirtualKeySetting _setting;
        public VirtualKeySetting Setting => _setting;

        public VirtualKey(VirtualKeySetting setting)
        {
            _setting = setting;
        }

        public void Push()
        {
            ///TODO: imple virtual clicking.
        }
    }
}