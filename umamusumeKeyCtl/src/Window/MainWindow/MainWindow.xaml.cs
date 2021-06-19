using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using OpenCvSharp;
using umamusumeKeyCtl.AppSettings;
using umamusumeKeyCtl.CaptureScene;
using umamusumeKeyCtl.Properties;
using Brush = System.Windows.Media.Brush;
using Window = System.Windows.Window;

namespace umamusumeKeyCtl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private CancellationTokenSource _tokenSource;
        private SceneSettingViewer _sceneSettingViewer;
        private SceneSelector _sceneSelector;
        private SceneViewer _sceneViewer;
        private DataGridWindow _debugWindow;
        private MainWndState _state = MainWndState.Default;
        public MainWndState State => _state;
        private readonly object _setStateLock = new();

        public event EventHandler<MainWndState> MainWndStateChanged;

        public MainWindow()
        {
            Initialize();

            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CaptionPanel_OnCloseWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, CaptionPanel_OnMinimizeWindow, OnCanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, CaptionPanel_OnRestoreWindow, OnCanResizeWindow));
            
            var _vm = new MainWndVM();

            this.DataContext = _vm;

            SetUpWindowCapture(_vm);

            _ = SceneHolder.Instance;

            _ = VirtualKeyPushExecutor.Instance;
        }

        public void SetState(MainWndState state)
        {
            lock (_setStateLock)
            {
                if (state != _state)
                {
                    _state = state;
                    MainWndStateChanged?.Invoke(this, _state);
                }
            }
        }

        private void Initialize()
        {
            _tokenSource = new CancellationTokenSource();
            
            InitializeComponent();
            
            _sceneSettingViewer = new SceneSettingViewer();
            
            SceneSettingHolder.Instance.OnLoadSettings += settingSets =>
            {
                this.Dispatcher.InvokeAsync(() => _sceneSettingViewer.OnLoadSettings(settingSets, Canvas, ToolPanel, SettingsView));
            };
            
            ((INotifyCollectionChanged)SettingsView.Items).CollectionChanged += (_, _) =>
            {
                SettingsView.UpdateLayout();
                ChangeColor(_tokenSource.Token);
            };
            
            // Instantiate SceneViewer
            _sceneViewer = new SceneViewer(Canvas);

            // Initialize sceneSelector
            _sceneSelector = new SceneSelector();
            Settings.Default.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName != null && args.PropertyName == "IsDebugMode")
                {
                    if (Settings.Default.IsDebugMode)
                    {
                        // Instantiate debugWindow
                        _debugWindow = new DataGridWindow();
                        _debugWindow.Show();
                    }
                    else
                    {
                        _debugWindow?.Close();
                        _debugWindow = null;
                    }
                    
                    Cv2.DestroyAllWindows();
                    _sceneSelector.IsDebugMode = Settings.Default.IsDebugMode;
                }
            };
            _sceneSelector.OnGetMatchingResults += (_, list) => this.Dispatcher.InvokeAsync(() => _debugWindow?.Vm.UpdateResults(list));
            _sceneSelector.SceneSelected += (_, scene) => this.Dispatcher.InvokeAsync(() => _sceneViewer.DrawScene(scene));
            _sceneSelector.ResultPrinted += (_, mat) => this.Dispatcher.Invoke(() => Cv2.ImShow("MatchingResult", mat));
            _sceneSelector.SrcTgtImgPrinted += (_, mat) => this.Dispatcher.InvokeAsync(() => Cv2.ImShow("SrcTgtImg", mat));

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

            windowCapture.CaptureResultObservable.Subscribe(source =>
            {
                vm.OnPrintWnd((Bitmap) source.Clone());
                
                _ = Task.Run(() =>
                {
                    try
                    {
                        _sceneSelector.SelectScene((Bitmap) source.Clone()).ContinueWith(_ => source.Dispose());
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        throw;
                    }
                }, _tokenSource.Token);
                
                this.Dispatcher.InvokeAsync(() =>
                {
                    Canvas.Width = Image.Width;
                    Canvas.Height = Image.Height;
                });

            }, exception => Console.Write(exception));

            Closing += (_, _) => windowCapture.Dispose();
        }

        private async void ChangeColor(CancellationToken token)
        {
            await WaitForContainer(SettingsView.ItemContainerGenerator, token);

            try
            {
                for (int i = 0; i < SettingsView.ItemContainerGenerator.Items.Count; i++)
                {
                    ListViewItem lbi = SettingsView.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;

                    if (lbi == null)
                    {
                        continue;
                    }
                
                    var converter = new BrushConverter();
                    lbi.Foreground = (Brush) converter.ConvertFromString("#f1f1f1");
                    lbi.Background = (Brush) converter.ConvertFromString("#3f4240");
                    lbi.BorderBrush = (Brush) converter.ConvertFromString("#535755");
                    lbi.BorderThickness = new Thickness(0.2);
                    lbi.Margin = new Thickness(0);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        private async Task WaitForContainer(ItemContainerGenerator generator, CancellationToken token)
        {
            while (generator.Status != GeneratorStatus.ContainersGenerated && token.IsCancellationRequested == false)
            {
                await Task.Delay(1);
            }
        }

        #region CaptionPanelUIEvents

        private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode != ResizeMode.NoResize;
        }

        private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private void CaptionPanel_OnCloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void CaptionPanel_OnMinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void CaptionPanel_OnRestoreWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        private void CaptionPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        #endregion

        #region SceneSettingsUIEvents

        private void SceneSettingsContextMenu_OnSaveSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            SceneSettingHolder.Instance.SaveSettings();
        }

        public void SceneSettingsContextMenu_OnCreateNewButtonClick(object sender, RoutedEventArgs e)
        {
            if (_state != MainWndState.Default)
            {
                new MessageWindow("設定の作成中または編集中です。").ShowDialog();
                return;
            }
            
            SetState(MainWndState.CreatingSetting);
            
            new SceneSettingMaker(this, Canvas, Canvas);
        }

        private void SceneSettingsContextMenu_OnReloadButtonClick(object sender, RoutedEventArgs e)
        {
            if (_state != MainWndState.Default)
            {
                new MessageWindow("設定の作成中または編集中です。").ShowDialog();
                return;
            }
            
            SceneSettingHolder.Instance.LoadSettings();
        }

        private void SceneSettingsContextMenu_OnRemoveButtonClick(object sender, RoutedEventArgs e)
        {
            if (_state != MainWndState.Default)
            {
                new MessageWindow("設定の作成中または編集中です。").ShowDialog();
                return;
            }
            
            _sceneSettingViewer.ModifyMode = false;
            _sceneSettingViewer.RemoveMode = true;
        }
        
        public void SceneSettingsContextMenu_OnModifyButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_state != MainWndState.Default)
            {
                new MessageWindow("設定の作成中または編集中です。").ShowDialog();
                return;
            }
            
            _sceneSettingViewer.RemoveMode = false;
            _sceneSettingViewer.ModifyMode = true;
        }

        public void OnChangeCollapsedButtonClick(object sender, RoutedEventArgs args)
        {
            SettingsView.Visibility = SettingsView.Visibility == Visibility.Collapsed
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void SceneSettingsListView_OnScroll(object sender, ScrollEventArgs e)
        {
            SettingsView.UpdateLayout();
            ChangeColor(_tokenSource.Token);
        }

        #endregion

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
            _tokenSource.Cancel();
            base.OnClosing(e);
        }

        public void Dispose()
        {
            _tokenSource?.Dispose();
        }
    }
}