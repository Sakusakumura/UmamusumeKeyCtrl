using System.Collections.Generic;

namespace umamusumeKeyCtl.CaptureScene
{
    public class SceneSetting
    {
        private string _name;
        public string Name => _name;
        
        private List<VirtualKeySetting> _virtualKeySettings;
        public List<VirtualKeySetting> VirtualKeySettings => _virtualKeySettings;

        private ScrapSetting _scrapSetting;
        public ScrapSetting ScrapSetting => _scrapSetting;

        public SceneSetting(string name, List<VirtualKeySetting> virtualKeySettings, ScrapSetting scrapSetting)
        {
            _name = name;
            _virtualKeySettings = virtualKeySettings;
            _scrapSetting = scrapSetting;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}