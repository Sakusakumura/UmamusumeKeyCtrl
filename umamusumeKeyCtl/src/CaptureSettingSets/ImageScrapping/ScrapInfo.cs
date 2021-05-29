using System.Windows;

namespace umamusumeKeyCtl.CaptureSettingSets.ImageScrapping
{
    public class ScrapInfo
    {
        private int _index;
        public int Index => _index;
        
        private Rect _scrapArea;
        public Rect ScrapArea => _scrapArea;

        public ScrapInfo(int index, Rect scrapArea)
        {
            this._index = index;
            this._scrapArea = scrapArea;
        }
    }
}