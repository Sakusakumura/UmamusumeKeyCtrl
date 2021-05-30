using umamusumeKeyCtl.AppSettings.SettingUI;

namespace umamusumeKeyCtl.AppSettings.Factory
{
    public abstract class SettingUIFactoryBase
    {
        public SettingUIBase Create(AppSettingDescription description)
        {
            SettingUIBase settingUiBase = CreateSettingUiBase(description);
            return settingUiBase;
        }

        protected abstract SettingUIBase CreateSettingUiBase(AppSettingDescription description);
    }
}