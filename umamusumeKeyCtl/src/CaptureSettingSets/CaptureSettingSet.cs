using System.Collections.Generic;
using umamusumeKeyCtl.CaptureSettingSets.ImageScrapping;
using umamusumeKeyCtl.CaptureSettingSets.VirtualKeyPushing;

namespace umamusumeKeyCtl.CaptureSettingSets
{
    public class CaptureSettingSet
    {
        private string _name;
        public string Name => _name;
        
        private List<VirtualKeySetting> _virtualKeySettings;
        public List<VirtualKeySetting> VirtualKeySettings => _virtualKeySettings;

        private ScrapSetting _scrapSetting;
        public ScrapSetting ScrapSetting => _scrapSetting;

        public CaptureSettingSet(string name, List<VirtualKeySetting> virtualKeySettings, ScrapSetting scrapSetting)
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