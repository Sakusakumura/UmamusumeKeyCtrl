using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using umamusumeKeyCtl.Annotations;
using Image = System.Windows.Controls.Image;

namespace umamusumeKeyCtl
{
    public class MainWndVM : INotifyPropertyChanged
    {
        private BitmapImage _myImage;
        public BitmapImage MyImage
        {
            get => _myImage;
            private set
            {
                _myImage = value;
                OnPropertyChanged("MyImage");
            }
        }

        private int wndHeight;

        public int WndHeight
        {
            get => wndHeight;
            set
            {
                wndHeight = value;
                OnPropertyChanged("WndHeight");
            }
        }
        
        private int wndWidth;

        public int WndWidth
        {
            get => wndWidth;
            set
            {
                wndWidth = value;
                OnPropertyChanged("WndWidth");
            }
        }

        public MainWndVM()
        {
            MyImage = BitmapToImageSource((Bitmap) Bitmap.FromFile("devilman.jpg"));
        }

        public void OnPrintWnd(Bitmap image)
        {
            var bitmap = BitmapToImageSource(image);
            
            MyImage = new BitmapImage();
            MyImage = bitmap;

            WndHeight = MyImage.PixelHeight + 9;
            WndWidth = MyImage.PixelWidth;
            
            image.Dispose();
        }
            
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            BitmapImage bitmapimage = new BitmapImage();
            
            using (MemoryStream memory = new MemoryStream())
            {
                try
                {
                    bitmap.Save(memory, ImageFormat.Bmp);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                memory.Position = 0;
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
            }
            
            return bitmapimage;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}