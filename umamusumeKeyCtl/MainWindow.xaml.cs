using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using umamusumeKeyCtl.Annotations;
using umamusumeKeyCtl.AppSettings;
using umamusumeKeyCtl.CaptureScene;
using umamusumeKeyCtl.Properties;

namespace umamusumeKeyCtl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private CancellationTokenSource _tokenSource;
        private SceneSettingViewer _sceneSettingViewer;

        public MainWindow()
        {
            Initialize();

            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));
            
            var _vm = new MainWndVM();

            this.DataContext = _vm;

            SetUpWindowCapture(_vm);

            _ = SampleImageHolder.Instance;

            _ = SceneHolder.Instance;
        }

        private void Initialize()
        {
            _tokenSource = new CancellationTokenSource();
            
            SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            
            InitializeComponent();
            
            _sceneSettingViewer = new SceneSettingViewer();
            
            SceneSettingHolder.Instance.OnLoadSettings += settingSets => _sceneSettingViewer.OnLoadSettings(settingSets, canvas, ToolPanel, SettingsView);
            
            ((INotifyCollectionChanged)SettingsView.Items).CollectionChanged += (sender, args) =>
            {
                SettingsView.UpdateLayout();
                ChangeColor(sender, args, _tokenSource.Token);
            };
            
            //Load settings
            SceneSettingHolder.Instance.LoadSettings();
            
            //Draw Application setting panel
            new AppSettingsUILoader(AppSettingsView).LoadAndDraw();
        }

        private void SetUpWindowCapture(MainWndVM vm)
        {
            var captureSetting = new CaptureSetting(Settings.Default.CaptureInterval, Settings.Default.CaptureWindowTitle);
            Settings.Default.PropertyChanged += (_, _) =>
            {
                captureSetting.Interval = Settings.Default.CaptureInterval;
                captureSetting.CaptureWndName = Settings.Default.CaptureWindowTitle;
            };

            var windowCapture = new WindowCapture(captureSetting);

            windowCapture.CaptureResultObservable.Subscribe(bitmap =>
            {
                vm.OnPrintWnd(bitmap);
                this.Dispatcher.Invoke(() =>
                {
                    canvas.Width = Image.Width;
                    canvas.Height = Image.Height;
                });
            }, exception => Console.Write(exception));

            Closing += (_, _) => windowCapture.StopCapture();
            Closing += (_, _) => SampleImageHolder.Instance.Dispose();
        }

        private async void ChangeColor([CanBeNull] object sender, NotifyCollectionChangedEventArgs args, CancellationToken token)
        {
            await WaitForContainer(SettingsView.ItemContainerGenerator, token);
            
            for (int i = 0; i < SettingsView.ItemContainerGenerator.Items.Count; i++)
            {
                ListViewItem lbi = SettingsView.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                    
                var converter = new BrushConverter();
                lbi.Foreground = (Brush) converter.ConvertFromString("#f1f1f1");
                lbi.Background = (Brush) converter.ConvertFromString("#3f4240");
                lbi.BorderBrush = (Brush) converter.ConvertFromString("#535755");
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
            new SceneSettingMaker(canvas, canvas);
        }

        private void OnSaveSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            SceneSettingHolder.Instance.SaveSettings();
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
            SceneSettingHolder.Instance.LoadSettings();
        }

        private void OnRemoveButtonClick(object sender, RoutedEventArgs e)
        {
            _sceneSettingViewer.ModifyMode = false;
            _sceneSettingViewer.RemoveMode = true;
        }
        
        public void OnModifyButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            _sceneSettingViewer.RemoveMode = false;
            _sceneSettingViewer.ModifyMode = true;
        }

        public void OnChangeCollapsedButtonClick(object sender, RoutedEventArgs args)
        {
            SettingsView.Visibility = SettingsView.Visibility == Visibility.Collapsed
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public void OnToggleSettingViewButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var visibility = MainPanel.Visibility == Visibility.Visible;

            if (visibility == false)
            {
                Settings.Default.Save();
            }

            MainPanel.Visibility = visibility ? Visibility.Hidden : Visibility.Visible;
            AppSettingsPanel.Visibility = visibility ? Visibility.Visible : Visibility.Hidden;
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