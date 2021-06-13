using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace umamusumeKeyCtl.AppSettings.SettingUI
{
    public class BooleanSettingUi : SettingUIBase
    {
        public BooleanSettingUi(AppSettingDescription description) : base(description)
        {
        }

        protected override UIElement CreateControl(GraphicalAppSetting bindingTarget)
        {
            var button = new CheckBox()
            {
                IsChecked = (bool) bindingTarget.SettingValue,
                Margin = new Thickness(5)
            };

            var valueButtonBinding = new Binding("SettingValue");
            valueButtonBinding.Source = bindingTarget;
            button.SetBinding(CheckBox.IsCheckedProperty, valueButtonBinding);

            return button;
        }
    }
}