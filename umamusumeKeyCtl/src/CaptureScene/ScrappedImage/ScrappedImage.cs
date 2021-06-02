using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace umamusumeKeyCtl.CaptureScene
{
    public class ScrappedImage : IDisposable
    {
        public ScrapSetting ScrapSetting { get; }

        public Bitmap[] ScrappedImages { get; }

        public ScrappedImage(Bitmap source, ScrapSetting setting)
        {
            ScrapSetting = setting;
            ScrappedImages = CropImage(source, setting.ScrapInfos);
        }
        
        private Bitmap[] CropImage(Bitmap source, List<ScrapInfo> scrapInfos)
        {
            var bitmaps = new List<Bitmap>();
            
            foreach (var scrapInfo in scrapInfos)
            {
                var bitmap = source.PerformCrop(scrapInfo.ScrapArea.ToRectangle());
                bitmaps.Add(bitmap);
            }

            return bitmaps.ToArray();
        }

        public void Dispose()
        {
            ScrappedImages?.ToList().ForEach(val => val?.Dispose());
        }
    }
}