using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using umamusumeKeyCtl.Annotations;
using umamusumeKeyCtl.CaptureSettingSets;
using umamusumeKeyCtl.UserInput;

namespace umamusumeKeyCtl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private CancellationTokenSource _tokenSource;

        public MainWindow()
        {
            LowLevelKeyboardListener.Instance.HookKeyboard();

            _tokenSource = new CancellationTokenSource();
            
            SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            
            InitializeComponent();

            // Handle event on settings are loaded
            CaptureSettingSetsHolder.Instance.OnLoadSettings += sets =>
            {
                SettingsView.ItemsSource = sets;
            };

            ((INotifyCollectionChanged)SettingsView.Items).CollectionChanged += (sender, args) =>
            {
                SettingsView.UpdateLayout();
                ChangeColor(sender, args, _tokenSource.Token);
            };
            
            //Load settings
            CaptureSettingSetsHolder.Instance.LoadSettings();

            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));


            var _vm = new MainWndVM();
            
            var windowCapture = new WindowCapture(Properties.Settings.Default.CaptureWindowTitle, Properties.Settings.Default.CaptureInterval);

            windowCapture.CaptureResultObservable.Subscribe(bitmap =>
            {
                using (bitmap)
                {
                    _vm.OnPrintWnd(bitmap);
                }
            }, exception => Console.Write(exception));
            windowCapture.CaptureResultObservable.Subscribe(bitmap =>
            {
                using (bitmap)
                {
                    canvas.Width = Image.Width;
                    canvas.Height = Image.Height;
                }
            }, exception => Console.Write(exception));

            Closing += (sender, args) => windowCapture.StopCapture();
            Closing += (sender, args) => SampleImageHolder.Instance.Dispose();

            this.DataContext = _vm;

            var _ = SampleImageHolder.Instance;
        }

        private async void ChangeColor([CanBeNull] object sender, NotifyCollectionChangedEventArgs args, CancellationToken token)
        {
            await WaitForContainer(SettingsView.ItemContainerGenerator, token);
            
            for (int i = 0; i < SettingsView.ItemContainerGenerator.Items.Count; i++)
            {
                ListViewItem lbi = SettingsView.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                    
                var converter = new BrushConverter();
                lbi.Foreground = (Brush) converter.ConvertFromString("#f1f1f1");
                lbi.Background = (Brush) converter.ConvertFromString("#535755");
                lbi.BorderBrush = (Brush) converter.ConvertFromString("#6f7472");
                lbi.BorderThickness = new Thickness(0.2);
                lbi.Margin = new Thickness(0);
            }
        }

        private async Task WaitForContainer(ItemContainerGenerator generator, CancellationToken token)
        {
            while (generator.Status != GeneratorStatus.ContainersGenerated && token.IsCancellationRequested == false)
            {
                await Task.Delay(1);
            }
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LowLevelKeyboardListener.Instance.UnHookKeyboard();
        }
        
        [DllImport("User32.dll")]
        public static extern DPI_AWARENESS_CONTEXT SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT dpiContext);

        public enum DPI_AWARENESS_CONTEXT
        {
            DPI_AWARENESS_CONTEXT_DEFAULT = 0,
            DPI_AWARENESS_CONTEXT_UNAWARE = -1,
            DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = -2, 
            DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = -3,
            DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4,
            DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED = -5
        }

        public void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var settingMaker = new CaptureSettingSetMaker(canvas, canvas);
            settingMaker.OnCaptureSettingSetCreated += CaptureSettingSetsHolder.Instance.AddSettings;
        }

        private void OnSaveSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            CaptureSettingSetsHolder.Instance.SaveSettings();
        }

        private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode != ResizeMode.NoResize;
        }

        private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private void OnCloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void OnMinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void OnRestoreWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        private void CaptionPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void OnReloadButtonClick(object sender, RoutedEventArgs e)
        {
            CaptureSettingSetsHolder.Instance.LoadSettings();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SampleImageHolder.Instance.Dispose();
            _tokenSource.Cancel();
            base.OnClosing(e);
        }

        public void Dispose()
        {
            _tokenSource?.Dispose();
        }
    }
}