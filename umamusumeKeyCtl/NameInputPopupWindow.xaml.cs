using System;
using System.Windows;
using System.Windows.Input;
using umamusumeKeyCtl.CaptureSettingSets;

namespace umamusumeKeyCtl
{
    /// <summary>
    /// NameInputPopupWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class NameInputPopupWindow : Window
    {
        public event Action<string> OnConfirm; 
        public event Action OnCanceled;
        
        public NameInputPopupWindow()
        {
            InitializeComponent();
            
            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));
        }

        private void OnConfirmButtonEvent(object sender, RoutedEventArgs e)
        {
            if (CaptureSettingSetsHolder.Instance.Settings.Exists(val => val.Name == NameTextBox.Text))
            {
                ErrorLabel.Content = "重複した設定名です";
                return;
            }
            
            OnConfirm?.Invoke(NameTextBox.Text);
            this.Close();
        }

        private void OnCancelButtonEvent(object sender, RoutedEventArgs e)
        {
            OnCanceled?.Invoke();
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
            SampleImageHolder.Instance.Dispose();
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
