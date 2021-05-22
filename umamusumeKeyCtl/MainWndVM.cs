using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using umamusumeKeyCtl.Annotations;
using Image = System.Windows.Controls.Image;

namespace umamusumeKeyCtl
{
    public class MainWndVM : INotifyPropertyChanged
    {
        [DllImport("gdi32")]
        public static extern bool DeleteObject(IntPtr hObject);
        
        private BitmapSource _myImage;
        public BitmapSource MyImage
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
            //MyImage = BitmapToBitmapSource(image);

            MyImage = BitmapToImageSource(image);

            WndHeight = MyImage.PixelHeight + 9;
            WndWidth = MyImage.PixelWidth;
        }
            
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            BitmapImage bitmapimage = new BitmapImage();
            
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                bitmap.Dispose();
                memory.Position = 0;
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                bitmapimage.Freeze();
            }
            
            return bitmapimage;
        }

        private BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            IntPtr hbitmap = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            bitmap.Dispose();
            DeleteObject(hbitmap);
            
            return bitmapSource;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}