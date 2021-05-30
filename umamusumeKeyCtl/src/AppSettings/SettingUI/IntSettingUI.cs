using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace umamusumeKeyCtl.AppSettings.SettingUI
{
    public class IntSettingUi : SettingUIBase
    {
        public IntSettingUi(AppSettingDescription description) : base(description)
        {
        }

        protected override UIElement CreateControl(GraphicalAppSetting bindingTarget)
        {
            var converter = new BrushConverter();

            var textBlock = new TextBox()
            {
                Text = ((int) bindingTarget.SettingValue).ToString(),
                Foreground = (SolidColorBrush) converter.ConvertFromString("#f4f5f4"),
                Background = (SolidColorBrush) converter.ConvertFromString("#535755"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5),
            };

            textBlock.PreviewTextInput += (sender, args) =>
            {
                // Allow only digits
                var regex = new Regex(@"^[1-9][0-9]*$");

                var inserted = textBlock.Text.Insert(textBlock.CaretIndex, args.Text);

                var isMatch = regex.IsMatch(inserted);
                args.Handled = !isMatch;
                Debug.Print($"inserted={inserted}, isMatch={isMatch}");
            };

            var binding = new Binding("SettingValue");
            binding.Source = bindingTarget;
            textBlock.SetBinding(TextBlock.TextProperty, binding);
            textBlock.LostFocus += (_, _) =>
            {
                bindingTarget.SettingValue = int.Parse(textBlock.Text);
            };

            return textBlock;
        }
    }
}