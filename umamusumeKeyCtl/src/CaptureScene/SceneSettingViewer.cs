using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        
        public event Action<bool> OnChangeRemoveMode;
        public event Action<bool> OnChangeModifyMode;

        public void OnLoadSettings(List<SceneSetting> captureSettingSets, Canvas canvas, StackPanel toolPanel, ListView listView)
        {
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

                // Will be added remove and modify label.
                var grid = new Grid();

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
                    (_, _) => SceneSettingHolder.Instance.RemoveSetting(setting.Name);
                removeLabel.Visibility = Visibility.Hidden;
                OnChangeRemoveMode += b => removeLabel.Visibility = b ? Visibility.Visible : Visibility.Hidden;

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
                    new SceneSettingModifier(setting, canvas, toolPanel);
                    OnChangeModifyMode.Invoke(false);
                };
                modifyLabel.Visibility = Visibility.Hidden;
                OnChangeModifyMode += b => modifyLabel.Visibility = b ? Visibility.Visible : Visibility.Hidden;

                grid.Children.Add(removeLabel);
                grid.Children.Add(modifyLabel);

                // setting name label.
                var label = new Label()
                {
                    Content = setting.Name,
                    Foreground = (Brush) converter.ConvertFromString("#e9eaea"),
                    Background = Brushes.Transparent,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Left
                };

                panel.Children.Add(grid);
                panel.Children.Add(label);

                DockPanel.SetDock(grid, Dock.Right);
                DockPanel.SetDock(label, Dock.Right);

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
            cancelRemoveLabel.Visibility = Visibility.Hidden;
            OnChangeRemoveMode += b => cancelRemoveLabel.Visibility = b ? Visibility.Visible : Visibility.Hidden;

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
            cancelModifyLabel.Visibility = Visibility.Hidden;
            OnChangeModifyMode += b => cancelModifyLabel.Visibility = b ? Visibility.Visible : Visibility.Hidden;

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
    }
}