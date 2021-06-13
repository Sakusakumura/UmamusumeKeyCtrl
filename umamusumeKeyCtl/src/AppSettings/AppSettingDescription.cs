namespace umamusumeKeyCtl.AppSettings
{
    public class AppSettingDescription
    {
        public string SettingName { get; }
        public string SettingType { get; }
        public string Title { get; }
        public string Description { get; }

        public AppSettingDescription(string settingName, string settingType, string title, string description)
        {
            Title = title;
            Description = description;
            SettingName = settingName;
            SettingType = settingType;
        }
    }
}