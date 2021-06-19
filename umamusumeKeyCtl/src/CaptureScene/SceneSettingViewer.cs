using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using umamusumeKeyCtl.ImageSimilarity.Factory;
using umamusumeKeyCtl.ImageSimilarity.Method;
using umamusumeKeyCtl.Util;

namespace umamusumeKeyCtl.CaptureScene
{
    /// <summary>
    /// CaptureSettingSetをListViewerで表示するためのクラス。
    /// </summary>
    public class SceneSettingViewer
    {
        private bool _removeMode = false;

        public bool RemoveMode
        {
            get => _removeMode;
            set
            {
                _removeMode = value;
                OnChangeRemoveMode.Invoke(_removeMode);
            }
        }
        
        private bool _modifyMode = false;

        public bool ModifyMode
        {
            get => _modifyMode;
            set
            {
                _modifyMode = value;
                OnChangeModifyMode.Invoke(_modifyMode);
            }
        }

        private ListView _settingsView;
        
        public event Action<bool> OnChangeRemoveMode;
        public event Action<bool> OnChangeModifyMode;

        public SceneSettingViewer()
        {
            var currentMainWindow = (MainWindow) Application.Current.MainWindow;
            _settingsView = currentMainWindow.SettingsView;
        }

        public void OnLoadSettings(List<SceneSetting> captureSettingSets, Canvas canvas, StackPanel toolPanel, ListView listView)
        {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA)
            {
                Debug.Print($"[{this.GetType().Name}] MTAのスレッドで実行されています。STAで実行してください。");
                return;
            }
            
            var mainWindow = (MainWindow) Application.Current.MainWindow;
            
            var dockPanels = new List<DockPanel>();
            var converter = new BrushConverter();

            for (var i = 0; i < captureSettingSets.Count; i++)
            {
                var setting = captureSettingSets[i];

                var panel = new DockPanel()
                {
                    Background = Brushes.Transparent,
                    LastChildFill = true,
                };

                // Create util labels.
                var utilLabelsRoot = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0),
                };
                
                // Create method label.
                var methodLabelRoot = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                
                var detectorMethodBorder = new Border()
                {
                    Background = GetDetectorMethodColor(setting.DetectorMethod),
                    CornerRadius = new CornerRadius(4.0),
                    Margin = new Thickness(0),
                    Height = 14,
                };
                var detectorMethodLabel = new Label()
                {
                    Content = setting.DetectorMethod.ToString(),
                    FontSize = 7,
                    Foreground = (Brush) converter.ConvertFromString("#ffffff"),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                var detectorMethodGrid = new Grid()
                {
                    Margin = new Thickness(0, 0, 1, 0)
                };
                detectorMethodGrid.Children.Add(detectorMethodBorder);
                detectorMethodGrid.Children.Add(detectorMethodLabel);
                methodLabelRoot.Children.Add(detectorMethodGrid);

                var descriptorMethodBorder = new Border()
                {
                    Background = GetDescriptorMethodColor(setting.DescriptorMethod),
                    CornerRadius = new CornerRadius(4.0),
                    Margin = new Thickness(0),
                    Height = 14,
                };
                var descriptorMethodLabel = new Label()
                {
                    Content = setting.DescriptorMethod.ToString(),
                    FontSize = 7,
                    Foreground = (Brush) converter.ConvertFromString("#ffffff"),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Left
                };
                var descriptorMethodGrid = new Grid()
                {
                    Margin = new Thickness(1, 0, 0, 0)
                };
                descriptorMethodGrid.Children.Add(descriptorMethodBorder);
                descriptorMethodGrid.Children.Add(descriptorMethodLabel);
                methodLabelRoot.Children.Add(descriptorMethodGrid);

                // Create remove and modify label.
                var removeModifyLabelRoot = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0),
                };

                var removeLabel = new Label()
                {
                    Content = "\uECC9",
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    FontSize = 15,
                    Foreground = (Brush) converter.ConvertFromString("#ff5c5c"),
                    Background = Brushes.Transparent,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    BorderThickness = new Thickness(0),
                };
                removeLabel.MouseLeftButtonUp +=
                    (_, _) => SceneSettingHolder.Instance.RemoveSetting(setting.Guid);
                removeLabel.Visibility = Visibility.Collapsed;
                OnChangeRemoveMode += b => removeLabel.Visibility = b ? Visibility.Visible : Visibility.Collapsed;

                var modifyLabel = new Label()
                {
                    Content = "\uE70F",
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    FontSize = 15,
                    Foreground = (Brush) converter.ConvertFromString("#e9eaea"),
                    Background = Brushes.Transparent,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    BorderThickness = new Thickness(0),
                };
                modifyLabel.MouseLeftButtonUp += (_, _) =>
                {
                    mainWindow.SetState(MainWndState.ModifyingSetting);
                    
                    new SceneSettingModifier(setting, canvas, toolPanel);
                    OnChangeModifyMode.Invoke(false);

                    panel.Visibility = Visibility.Collapsed;
                    _settingsView.Visibility = _settingsView.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                };
                modifyLabel.Visibility = Visibility.Collapsed;
                OnChangeModifyMode += b => modifyLabel.Visibility = b ? Visibility.Visible : Visibility.Collapsed;

                removeModifyLabelRoot.Children.Add(removeLabel);
                removeModifyLabelRoot.Children.Add(modifyLabel);

                utilLabelsRoot.Children.Add(methodLabelRoot);
                utilLabelsRoot.Children.Add(removeModifyLabelRoot);

                // setting name label.
                var displayNameLabel = new HoldableLabel()
                {
                    Content = setting.DisplayName,
                    Foreground = (Brush) converter.ConvertFromString("#e9eaea"),
                    Background = Brushes.Transparent,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    HoldDuration = TimeSpan.FromMilliseconds(500)
                };
                displayNameLabel.MouseLeftButtonHold += (_, _) =>
                {
                    var modLabelVis = modifyLabel.Visibility == Visibility.Visible;
                    var remLabelVis = removeLabel.Visibility == Visibility.Visible;
                    
                    if (modLabelVis || remLabelVis)
                    {
                        return;
                    }
                    
                    modifyLabel.Visibility = modLabelVis ? Visibility.Collapsed : Visibility.Visible;
                    removeLabel.Visibility = remLabelVis ? Visibility.Collapsed : Visibility.Visible;
                };
                listView.SelectionChanged += (_, _) =>
                {
                    if (modifyLabel.Visibility != Visibility.Visible || removeLabel.Visibility != Visibility.Visible)
                    {
                        return;
                    }
                    
                    modifyLabel.Visibility = Visibility.Collapsed;
                    removeLabel.Visibility = Visibility.Collapsed;
                };

                panel.Children.Add(utilLabelsRoot);
                panel.Children.Add(displayNameLabel);

                DockPanel.SetDock(utilLabelsRoot, Dock.Right);
                DockPanel.SetDock(displayNameLabel, Dock.Right);

                dockPanels.Add(panel);
            }

            // Cancel buttons
            var cancelRemoveLabel = new Label()
            {
                Content = "削除をキャンセル",
                Foreground = (Brush) converter.ConvertFromString("#e9eaea"),
                Background = Brushes.Transparent,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                BorderBrush = (Brush) converter.ConvertFromString("#e9eaea"),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };
            cancelRemoveLabel.MouseLeftButtonUp += (_, _) =>
            {
                listView.SelectedIndex = 0;
                RemoveMode = false;
            };
            cancelRemoveLabel.Visibility = Visibility.Collapsed;
            OnChangeRemoveMode += b => cancelRemoveLabel.Visibility = b ? Visibility.Visible : Visibility.Collapsed;

            var cancelModifyLabel = new Label()
            {
                Content = "編集をキャンセル",
                Foreground = (Brush) converter.ConvertFromString("#e9eaea"),
                Background = Brushes.Transparent,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                BorderBrush = (Brush) converter.ConvertFromString("#e9eaea"),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };
            cancelModifyLabel.MouseLeftButtonUp += (_, _) =>
            {
                listView.SelectedIndex = 0;
                ModifyMode = false;
            };
            cancelModifyLabel.Visibility = Visibility.Collapsed;
            OnChangeModifyMode += b => cancelModifyLabel.Visibility = b ? Visibility.Visible : Visibility.Collapsed;

            var cancelDockPanel = new DockPanel()
            {
                Height = 0,
                Background = Brushes.Transparent,
                LastChildFill = true
            };
            var cancelDockGrid = new Grid();
            cancelDockGrid.Children.Add(cancelRemoveLabel);
            cancelDockGrid.Children.Add(cancelModifyLabel);
            cancelDockPanel.Children.Add(cancelDockGrid);
            OnChangeModifyMode += b => cancelDockPanel.Height = b ? double.NaN : 0;
            OnChangeRemoveMode += b => cancelDockPanel.Height = b ? double.NaN : 0;

            dockPanels.Add(cancelDockPanel);

            // Add items to listView
            listView.ItemsSource = dockPanels;
        }

        private SolidColorBrush GetDetectorMethodColor(DetectorMethod detectorMethod)
        {
            var generator = new BrushConverter();
            
            if (detectorMethod == DetectorMethod.FAST)
            {
                return (SolidColorBrush) generator.ConvertFromString("#365fb7");
            }
            if (detectorMethod == DetectorMethod.ORB)
            {
                return (SolidColorBrush) generator.ConvertFromString("#b7369f");
            }
            if (detectorMethod == DetectorMethod.SIFT)
            {
                return (SolidColorBrush) generator.ConvertFromString("#b78e36");
            }

            throw new ArgumentException($"[{this.GetType().Name}] methodType={detectorMethod} is larger than max value of 2.");
        }
        
        private SolidColorBrush GetDescriptorMethodColor(DescriptorMethod methodType)
        {
            var generator = new BrushConverter();
            
            if (methodType == DescriptorMethod.BRIEF)
            {
                return (SolidColorBrush) generator.ConvertFromString("#36b74e");
            }
            if (methodType == DescriptorMethod.ORB)
            {
                return (SolidColorBrush) generator.ConvertFromString("#b7369f");
            }
            if (methodType == DescriptorMethod.SIFT)
            {
                return (SolidColorBrush) generator.ConvertFromString("#b78e36");
            }

            throw new ArgumentException($"[{this.GetType().Name}] methodType={methodType} is larger than max value of 2.");
        }
    }
}