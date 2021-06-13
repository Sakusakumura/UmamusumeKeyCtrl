using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace umamusumeKeyCtl.AppSettings.SettingUI
{
    public class StringSettingUi : SettingUIBase
    {
        public StringSettingUi(AppSettingDescription description) : base(description)
        {
        }

        protected override UIElement CreateControl(GraphicalAppSetting bindingTarget)
        {
            var converter = new BrushConverter();

            var textBlock = new TextBlock()
            {
                Foreground = (SolidColorBrush) converter.ConvertFromString("#f4f5f4"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5),
            };
            
            var binding = new Binding("SettingValue");
            binding.Source = bindingTarget;
            textBlock.SetBinding(TextBlock.TextProperty, binding);

            return textBlock;
        }
    }
}