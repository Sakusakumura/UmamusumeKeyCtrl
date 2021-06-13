using System;
using System.Collections.Generic;
using umamusumeKeyCtl.ImageSimilarity.Factory;

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

        private DetectorMethod _detectorMethod;
        public DetectorMethod DetectorMethod => _detectorMethod;

        private DescriptorMethod _descriptorMethod;
        public DescriptorMethod DescriptorMethod => _descriptorMethod;

        public SceneSetting(Guid guid, string displayName, List<VirtualKeySetting> virtualKeySettings, ScrapSetting scrapSetting, DetectorMethod detectorMethod, DescriptorMethod descriptorMethod)
        {
            _guid = guid;
            _displayName = displayName;
            _virtualKeySettings = virtualKeySettings;
            _scrapSetting = scrapSetting;
            _detectorMethod = detectorMethod;
            _descriptorMethod = descriptorMethod;
        }

        public override string ToString()
        {
            return _displayName;
        }
    }
}