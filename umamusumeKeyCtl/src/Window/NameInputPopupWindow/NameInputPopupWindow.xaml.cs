using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using umamusumeKeyCtl.CaptureScene;
using umamusumeKeyCtl.ImageSimilarity.Factory;

namespace umamusumeKeyCtl
{
    /// <summary>
    /// NameInputPopupWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class NameInputPopupWindow : Window
    {
        private char[] _invalidChars;
        
        private string errorMessage = "";
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnErrorMessageChanged(errorMessage);
            }
        }

        public event EventHandler<Tuple<string, DetectorMethod, DescriptorMethod>> Confirm; 
        public event EventHandler Canceled;
        
        public NameInputPopupWindow(bool isCreateNewMode, string defaultNameText = "", int defaultDetectorMethod = 0, int defaultExtructorMethod = 0)   
        {
            InitializeComponent();
            
            var invalid = (new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars())).ToCharArray().ToList();
            invalid.Remove('/');
            _invalidChars = invalid.ToArray();
            
            this.Owner = Application.Current.MainWindow;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            ErrorMessage = "";

            if (!isCreateNewMode)
            {
                ConfirmButton.Content = "変更";
            }

            NameTextBox.Text = defaultNameText;
            NameTextBox.PreviewTextInput += NameTextBoxOnPreviewTextInput;

            DetectorComboBox.SelectedIndex = defaultDetectorMethod;
            DescriberComboBox.SelectedIndex = defaultExtructorMethod;
            
            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));
        }

        private void NameTextBoxOnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (var character in _invalidChars)
            {
                if (e.Text.Contains(character))
                {
                    e.Handled = true;
                }
            }
        }

        private void OnConfirmButtonEvent(object sender, RoutedEventArgs e)
        {
            Confirm?.Invoke(
                this,
                new Tuple<string, DetectorMethod, DescriptorMethod>(
                    NameTextBox.Text,
                    (DetectorMethod) DetectorComboBox.SelectedIndex,
                    (DescriptorMethod) DescriberComboBox.SelectedIndex
                )
            );
            this.Close();
        }

        private void OnErrorMessageChanged(string str)
        {
            ErrorLabel.Content = str;
            
            if (String.IsNullOrEmpty(str) == false)
            {
                Height = 180;
                ErrorLabel.Height = 30;
                return;
            }

            Height = 150;
            ErrorLabel.Height = 0;
        }

        private void OnCancelButtonEvent(object sender, RoutedEventArgs e)
        {
            Canceled?.Invoke(this, e);
            this.Close();
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
    }
}
