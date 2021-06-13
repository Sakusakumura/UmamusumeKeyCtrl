using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using umamusumeKeyCtl.ImageSimilarity.Factory;
using umamusumeKeyCtl.ImageSimilarity.Method;

namespace umamusumeKeyCtl.CaptureScene
{
    public class ScrappedImage : IDisposable
    {
        public ScrapSetting Setting { get; }
        public Bitmap Image { get; }
        public DetectAndComputeResult FeaturePointsInfo { get; }
        public ReadOnlyCollection<ScrapInfo> SamplingAreas { get; }
        
        public ScrappedImage(Bitmap source, ScrapSetting setting, DetectorMethod detectorMethod, DescriptorMethod descriptorMethod)
        {
            Setting = setting;

            SamplingAreas = Setting.ScrapInfos.ToList().AsReadOnly();
            
            Image = CropImage((Bitmap) source.Clone(), setting.ScrapInfos);
            
            FeaturePointsInfo = new ImageSimilaritySearcher(detectorMethod, descriptorMethod).DetectAndCompete(Image);
        }
        
        /// <summary>
        /// for debuug.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="scrapInfos"></param>
        /// <returns></returns>
        private Bitmap CropImage(Bitmap source, List<ScrapInfo> scrapInfos)
        {
            using (source)
            {
                var root = new Bitmap(source.Width, source.Height);
                var rootRect = new Rectangle(0, 0, root.Width, root.Height);
                using (var graphics = Graphics.FromImage(root))
                {
                    foreach (var scrapInfo in scrapInfos)
                    {
                        var rect = scrapInfo.ScrapArea.ToRectangle();
                        rect.Intersect(rootRect);
                    
                        using var cloned = (Bitmap) source.Clone();
                        using var bitmap = cloned.PerformCrop(rect);
                    
                        graphics.DrawImage(bitmap, rect);
                    }
                }

                return root;
            }
        }

        public void Dispose()
        {
            Image?.Dispose();
        }
    }
}