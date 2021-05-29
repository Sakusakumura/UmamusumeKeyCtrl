using System.Collections.Generic;

namespace umamusumeKeyCtl.CaptureSettingSets.ImageScrapping
{
    public class ScrapSetting
    {
        private List<ScrapInfo> _scrapInfos;
        public List<ScrapInfo> ScrapInfos => _scrapInfos;

        public ScrapSetting(List<ScrapInfo> scrapInfos)
        {
            _scrapInfos = scrapInfos;
        }
    }
}