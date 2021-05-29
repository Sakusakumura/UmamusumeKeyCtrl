using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using umamusumeKeyCtl.Annotations;
using umamusumeKeyCtl.Properties;

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

        private int _toolPanelWidth;

        public int ToolPanelWidth
        {
            get => _toolPanelWidth;
            set
            {
                _toolPanelWidth = value;
                OnPropertyChanged("ToolPanelWidth");
            }
        }

        public MainWndVM()
        {
            MyImage = BitmapToImageSource((Bitmap) Bitmap.FromFile("devilman.jpg"));
            _toolPanelWidth = Settings.Default.ImageResolutionWidth;
        }

        public void OnPrintWnd(Bitmap image)
        {
            using (image)
            {
                MyImage = BitmapToImageSource(image);
            
                WndHeight = MyImage.PixelHeight;
                WndWidth = MyImage.PixelWidth + 100;
                ToolPanelWidth = 100;
            }
        }
            
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (bitmap)
            {
                BitmapImage bitmapimage = new BitmapImage();
            
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Bmp);
                    memory.Position = 0;
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();
                    bitmapimage.Freeze();
                }
            
                return bitmapimage;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}