using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace umamusumeKeyCtl.CaptureScene
{
    public class SceneSettingModifyToolBox
    {
        private StackPanel _toolPanel;
        private StackPanel _rootPanel;

        private EditMode _scrapSettingEditMode = EditMode.Modify;

        public EditMode ScrapSettingEditMode
        {
            get => _scrapSettingEditMode;
            set
            {
                _scrapSettingEditMode = value;
                OnScrapSettingModifyModeSelected.Invoke(_scrapSettingEditMode);
            }
        }

        private EditMode _virtualKeySettingEditMode = EditMode.Modify;

        public EditMode VirtualKeySettingEditMode
        {
            get => _virtualKeySettingEditMode;
            set
            {
                _virtualKeySettingEditMode = value;
                OnVirtualKeySettingModifyModeSelected.Invoke(_virtualKeySettingEditMode);
            }
        }

        public event Action OnModifyTitleClicked; 
        public event Action<EditMode> OnScrapSettingModifyModeSelected;
        public event Action<EditMode> OnVirtualKeySettingModifyModeSelected;
        public event Action OnFinishEditing;

        public SceneSettingModifyToolBox(StackPanel toolPanel)
        {
            _toolPanel = toolPanel;
            _toolPanel.Children.Add(CreateToolBox());
        }

        public void Discard()
        {
            if (_rootPanel == null)
            {
                return;
            }
            
            _toolPanel.Children.Remove(_rootPanel);
            _rootPanel = null;
        }

        private StackPanel CreateToolBox()
        {
            var converter = new BrushConverter();
            
            _rootPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
            };

            // Toolbox title
            var titlePanel = new DockPanel()
            {
                LastChildFill = true
            };

            var spaceLabel = new Label()
            {
                Width = 5,
                BorderBrush = (SolidColorBrush) converter.ConvertFromString("#535755"),
                BorderThickness = new Thickness(0, 0.5, 0, 0.25),
            };

            var titleButton = new Button()
            {
                Content = "編集オプション",
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Foreground = (SolidColorBrush) converter.ConvertFromString("#f1f1f1"),
                Background = Brushes.Transparent,
                BorderBrush = (SolidColorBrush) converter.ConvertFromString("#535755"),
                BorderThickness = new Thickness(0, 0.5, 0, 0.25),
            };

            titlePanel.Children.Add(spaceLabel);
            titlePanel.Children.Add(titleButton);

            // Modify title button
            var modifyTitleButton = new Button()
            {
                Content = "設定名変更",
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Foreground = (SolidColorBrush) converter.ConvertFromString("#ffffff"),
                Background = (SolidColorBrush) converter.ConvertFromString("#7d8380"),
                BorderBrush = (SolidColorBrush) converter.ConvertFromString("#343736"),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(5, 3, 5, 3)
            };
            modifyTitleButton.Click += (_, _) => OnModifyTitleClicked?.Invoke();

            // create other panel
            var scrapSettingPanel = CreateScrapSettingPanel();
            var virtualKeySettingPanel = CreateVirtualKeySettingPanel();
            var finishEditPanel = CreateFinishEditPanel();

            // Add elements
            _rootPanel.Children.Add(titlePanel);
            _rootPanel.Children.Add(modifyTitleButton);
            _rootPanel.Children.Add(scrapSettingPanel);
            _rootPanel.Children.Add(virtualKeySettingPanel);
            _rootPanel.Children.Add(finishEditPanel);
            
            // Set collapse movement
            titleButton.Click += (_, _) =>
            {
                modifyTitleButton.Visibility = modifyTitleButton.Visibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
                scrapSettingPanel.Visibility = scrapSettingPanel.Visibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
                virtualKeySettingPanel.Visibility = virtualKeySettingPanel.Visibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
                finishEditPanel.Visibility = finishEditPanel.Visibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            };

            return _rootPanel;
        }

        private StackPanel CreateScrapSettingPanel()
        {
            var converter = new BrushConverter();
            
            var scrapSettingToolMenu = new StackPanel()
            {
                Orientation = Orientation.Vertical,
            };

            scrapSettingToolMenu.Children.Add(new Label()
            {
                Content = "切り取り範囲",
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Foreground = (SolidColorBrush) converter.ConvertFromString("#f1f1f1"),
                Background = Brushes.Transparent,
                BorderBrush = (SolidColorBrush) converter.ConvertFromString("#535755"),
                BorderThickness = new Thickness(0, 0.5, 0, 0.25),
            });

            var scrapSettingRadioButtonPanel = new UniformGrid()
            {
                Columns = 3,
            };

            for (int i = 0; i < 3; i++)
            {
                var radioButton = new RadioButton()
                {
                    Style = (Style) Application.Current.Resources["ToggleButtonStyle"],
                    Content = IntToModifyMode[i]
                };
                
                if (i == 0)
                {
                    radioButton.IsChecked = true;
                }
                
                var temp = i;

                radioButton.Checked += (_, _) => ScrapSettingEditMode = (EditMode) temp;
                
                OnScrapSettingModifyModeSelected += mode =>
                {
                    if (temp == (int) mode)
                    {
                        radioButton.IsChecked = true;
                    }
                };

                scrapSettingRadioButtonPanel.Children.Add(radioButton);
            }

            scrapSettingToolMenu.Children.Add(scrapSettingRadioButtonPanel);

            return scrapSettingToolMenu;
        }
        
        private StackPanel CreateVirtualKeySettingPanel()
        {
            var converter = new BrushConverter();
            
            var virtualKeyToolMenu = new StackPanel()
            {
                Orientation = Orientation.Vertical,
            };

            virtualKeyToolMenu.Children.Add(new Label()
            {
                Content = "仮想キー",
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Foreground = (SolidColorBrush) converter.ConvertFromString("#f1f1f1"),
                Background = Brushes.Transparent,
                BorderBrush = (SolidColorBrush) converter.ConvertFromString("#535755"),
                BorderThickness = new Thickness(0, 0.5, 0, 0.25),
            });

            var virtualKeyRadioButtonPanel = new UniformGrid()
            {
                Columns = 3
            };

            for (int i = 0; i < 3; i++)
            {
                var radioButton = new RadioButton()
                {
                    Style = (Style) Application.Current.Resources["ToggleButtonStyle"],
                    Content = IntToModifyMode[i]
                };

                if (i == 0)
                {
                    radioButton.IsChecked = true;
                }

                var temp = i;
                
                radioButton.Checked += (_, _) => VirtualKeySettingEditMode = (EditMode) temp;

                OnVirtualKeySettingModifyModeSelected += mode =>
                {
                    if (temp == (int) mode)
                    {
                        radioButton.IsChecked = true;
                    }
                };

                virtualKeyRadioButtonPanel.Children.Add(radioButton);
            }

            virtualKeyToolMenu.Children.Add(virtualKeyRadioButtonPanel);

            return virtualKeyToolMenu;
        }

        private DockPanel CreateFinishEditPanel()
        {
            var converter = new BrushConverter();
            
            var panel = new DockPanel();

            var grid = new Grid();

            grid.Children.Add(new Label()
            {
                Content = "",
                HorizontalAlignment = HorizontalAlignment.Right,
                HorizontalContentAlignment = HorizontalAlignment.Right,
            });
            
            var finishButton = new Button()
            {
                Content = "編集を完了",
                Foreground = (Brush) converter.ConvertFromString("#f4f5f4"),
                Background = (Brush) converter.ConvertFromString("#6f7472"),
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                BorderThickness = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 1, 5, 1),
            };
            finishButton.Click += (_, _) => OnFinishEditing.Invoke();

            grid.Children.Add(finishButton);

            panel.Children.Add(grid);
            
            DockPanel.SetDock(grid, Dock.Right);

            return panel;
        }

        private Dictionary<int, string> IntToModifyMode = new()
        {
            {0, "編集"},
            {1, "追加"},
            {2, "削除"}
        };
    }
}