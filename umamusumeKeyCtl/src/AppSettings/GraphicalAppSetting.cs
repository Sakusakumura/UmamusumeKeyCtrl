using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using umamusumeKeyCtl.Annotations;
using umamusumeKeyCtl.Properties;

namespace umamusumeKeyCtl.AppSettings
{
    public class GraphicalAppSetting : INotifyPropertyChanged
    {
        private string _settingName;

        public string SettingName => _settingName;

        private object _settingValue;

        public object SettingValue
        {
            get => _settingValue;
            set
            {
                _settingValue = value;
                Settings.Default[SettingName] = _settingValue;
                OnPropertyChanged("SettingValue");
            }
        }

        private StackPanel _dockPanel;

        public StackPanel DockPanel => _dockPanel;

        private string _title;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        private string _description;

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        public GraphicalAppSetting(AppSettingDescription description, object settingValue, StackPanel dockPanel)
        {
            Title = description.Title;
            Description = description.Description;
            _settingName = description.SettingName;
            SettingValue = settingValue;
            _dockPanel = dockPanel;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}