using System.Windows;
using System.Windows.Input;

namespace umamusumeKeyCtl.CaptureSettingSets.VirtualKeyPushing
{
    public class VirtualKeySetting
    {
        private Key _bindKey;
        public Key BindKey => _bindKey;

        private Point _pressPos;
        public Point PressPos => _pressPos;

        public VirtualKeySetting(Key bindKey, Point pressPos)
        {
            _bindKey = bindKey;
            _pressPos = pressPos;
        }
    }
}