using System;
using System.Collections.Generic;
using System.Drawing;
using Size = System.Drawing.Size;

namespace umamusumeKeyCtl.CaptureSettingSets.ImageScrapping
{
    public class ScrapedImage : IDisposable
    {
        private List<ScrapInfo> _scrapInfos;
        public List<ScrapInfo> ScrapInfos => _scrapInfos;

        private Bitmap _source;
        public Bitmap Source => _source;

        private Bitmap _scrappedBitmap;

        public ScrapedImage(string name, List<ScrapInfo> scrapInfos, Bitmap source)
        {
            _scrapInfos = scrapInfos;
            _source = source;
        }

        public ScrapedImage(ScrapSetting setting, Bitmap source)
        {
            _scrapInfos = setting.ScrapInfos;
            _source = source;
        }

        public Bitmap ToBitmap()
        {
            if (_scrappedBitmap != null)
            {
                return _scrappedBitmap;
            }

            var bitmap = new Bitmap(_source.Width, _source.Height);

            //Make a black bitmap image.
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    bitmap.SetPixel(x, y, Color.Black);
                }
            }
            
            foreach (var scrapInfo in _scrapInfos)
            {
                var rect = scrapInfo.ScrapArea;
                for (int x = (int) rect.X; x < rect.X + rect.Width; x++)
                {
                    if (x > bitmap.Width)
                    {
                        break;
                    }

                    for (int y = (int) rect.Y; y < rect.Y + rect.Height; y++)
                    {
                        if (y > bitmap.Height)
                        {
                            break;
                        }

                        try
                        {
                            bitmap.SetPixel(x, y, Source.GetPixel(x - (int) rect.X, y - (int) rect.Y));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                }
            }

            _scrappedBitmap = bitmap;

            return bitmap;
        }

        public static explicit operator Bitmap(ScrapedImage image)
        {
            return image.ToBitmap();
        }

        public void Dispose()
        {
            _source?.Dispose();
            _scrappedBitmap?.Dispose();
        }
    }
}