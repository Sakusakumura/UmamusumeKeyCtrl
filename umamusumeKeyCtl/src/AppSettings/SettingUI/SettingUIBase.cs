using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using umamusumeKeyCtl.Properties;

namespace umamusumeKeyCtl.AppSettings.SettingUI
{
    public abstract class SettingUIBase
    {
        protected AppSettingDescription _description;

        protected SettingUIBase(AppSettingDescription description)
        {
            _description = description;
        }

        public GraphicalAppSetting GetSettingPanel()
        {
            var panel = new StackPanel();

            var graphicalAppSetting = new GraphicalAppSetting(_description, Settings.Default[_description.SettingName], panel);

            var converter = new BrushConverter();
            
            var titleLabel = new TextBlock()
            {
                Text = graphicalAppSetting.Title,
                Foreground = (SolidColorBrush) converter.ConvertFromString("#f4f5f4"),
                FontSize = 15,
                Margin = new Thickness(5),
            };

            var titleBinding = new Binding("Title");
            titleBinding.Source = graphicalAppSetting;
            titleLabel.SetBinding(TextBlock.TextProperty, titleBinding);

            var descriptionLabel = new TextBlock()
            {
                Text = graphicalAppSetting.Description,
                Foreground = (SolidColorBrush) converter.ConvertFromString("#f4f5f4"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5),
            };

            var descriptionBinding = new Binding("Description");
            descriptionBinding.Source = graphicalAppSetting;
            descriptionLabel.SetBinding(TextBlock.TextProperty, descriptionBinding);

            

            panel.Children.Add(titleLabel);
            panel.Children.Add(descriptionLabel);
            panel.Children.Add(CreateControl(graphicalAppSetting));

            return graphicalAppSetting;
        }

        protected abstract UIElement CreateControl(GraphicalAppSetting bindingTarget);
    }
}