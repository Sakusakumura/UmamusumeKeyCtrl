using System;
using System.Collections.Generic;

namespace umamusumeKeyCtl.CaptureScene
{
    public class SceneSetting
    {
        private Guid _guid;
        public Guid Guid => _guid;
        
        private string _displayName;
        public string DisplayName => _displayName;
        
        private List<VirtualKeySetting> _virtualKeySettings;
        public List<VirtualKeySetting> VirtualKeySettings => _virtualKeySettings;

        private ScrapSetting _scrapSetting;
        public ScrapSetting ScrapSetting => _scrapSetting;

        public SceneSetting(Guid guid, string displayName, List<VirtualKeySetting> virtualKeySettings, ScrapSetting scrapSetting)
        {
            _guid = guid;
            _displayName = displayName;
            _virtualKeySettings = virtualKeySettings;
            _scrapSetting = scrapSetting;
        }

        public override string ToString()
        {
            return _displayName;
        }
    }
}