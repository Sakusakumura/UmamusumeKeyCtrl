using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using umamusumeKeyCtl.CaptureSettingSets;
using umamusumeKeyCtl.Properties;

namespace umamusumeKeyCtl
{
    public class SampleImageHolder : Singleton<SampleImageHolder>, IDisposable
    {
        public Dictionary<string, Bitmap> Samples = new ();

        private bool _disposed = false;

        public SampleImageHolder()
        {
            Task.Run(() => LoadAssets());
        }

        ~SampleImageHolder()
        {
            Dispose(false);
        }

        private async Task LoadAssets()
        {
            List<Task<Bitmap>> tasks = new List<Task<Bitmap>>();

            Directory.CreateDirectory(Settings.Default.ScreenShotLocation);

            foreach (var captureSettingSet in CaptureSettingSetsHolder.Instance.Settings)
            {
                tasks.Add(LoadAsset($"{Settings.Default.ScreenShotLocation}/{captureSettingSet.Name}.bmp"));
            }

            try
            {
                await Task.Run(() => tasks);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            for (int i = 0; i < CaptureSettingSetsHolder.Instance.Settings.Count; i++)
            {
                Samples.Add(CaptureSettingSetsHolder.Instance.Settings[i].Name, tasks[i].Result);
                tasks[i].Dispose();
            }
        }

        private async Task<Bitmap> LoadAsset(string imagePath)
        {
            try
            {
                return await Task.Run(() => (Bitmap) Bitmap.FromFile(imagePath));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (Samples != null)
                {
                    foreach (var sample in Samples)
                    {
                        sample.Value.Dispose();
                    }

                    Samples = null;
                }
            }
            
            _disposed = true;
        }
    }
}