using System.Drawing;

namespace umamusumeKeyCtl.CaptureSettingSets.ImageScrapping
{
    public class ScrapSetting
    {
        private Rectangle[] _scrapInfos;
        public Rectangle[] ScrapInfos => _scrapInfos;

        public ScrapSetting(Rectangle[] scrapInfos)
        {
            _scrapInfos = scrapInfos;
        }
    }
}