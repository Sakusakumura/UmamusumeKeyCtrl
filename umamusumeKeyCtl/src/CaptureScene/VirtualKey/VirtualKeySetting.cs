using System.Windows;
using System.Windows.Input;

namespace umamusumeKeyCtl.CaptureScene
{
    public class VirtualKeySetting
    {
        private int _index;
        public int Index => _index;
        
        private Key _bindKey;
        public Key BindKey => _bindKey;

        private Point _pressPos;
        public Point PressPos => _pressPos;

        public VirtualKeySetting(int index, Key bindKey, Point pressPos)
        {
            _index = index;
            _bindKey = bindKey;
            _pressPos = pressPos;
        }
    }
}