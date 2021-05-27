using System;
using System.Drawing;
using Size = System.Drawing.Size;

namespace umamusumeKeyCtl.CaptureSettingSets.ImageScrapping
{
    public class ScrapedImage : IDisposable
    {
        private Rectangle[] _scrapInfos;
        public Rectangle[] ScrapInfos => _scrapInfos;

        private Bitmap _source;
        public Bitmap Source => _source;

        private Bitmap _scrappedBitmap;

        public ScrapedImage(string name, Rectangle[] scrapInfos, Bitmap source)
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
                for (int x = scrapInfo.X; x < scrapInfo.X + scrapInfo.Width; x++)
                {
                    if (x > bitmap.Width)
                    {
                        break;
                    }

                    for (int y = scrapInfo.Y; y < scrapInfo.Y + scrapInfo.Height; y++)
                    {
                        if (y > bitmap.Height)
                        {
                            break;
                        }

                        try
                        {
                            bitmap.SetPixel(x, y, Source.GetPixel(x - scrapInfo.X, y - scrapInfo.Y));
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