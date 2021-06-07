using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using umamusumeKeyCtl.Annotations;
using umamusumeKeyCtl.Properties;
using umamusumeKeyCtl.Util;

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
            _toolPanelWidth = Settings.Default.ImageResolutionWidth;
        }

        /// <summary>
        /// Convert Bitmap to BitmapImage, set WndHeight, WndWidth, and ToolPanelWidth. 
        /// </summary>
        /// <param name="image"></param>
        public void OnPrintWnd(Bitmap image)
        {
            MyImage = new WinFormsBitmapWrapper(image);
            //MyImage = image.ToBitmapSource();
            //MyImage = BitmapToImageSource(image);
            
            WndHeight = MyImage.PixelHeight;
            WndWidth = MyImage.PixelWidth + 100;
            ToolPanelWidth = 100;
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            try
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();
                    bitmapimage.Freeze();
                    return bitmapimage;
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                throw;
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