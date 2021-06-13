using System;
using umamusumeKeyCtl.AppSettings.SettingUI;
using umamusumeKeyCtl.Properties;

namespace umamusumeKeyCtl.AppSettings.Factory
{
    public class SettingUIFactory : SettingUIFactoryBase
    {
        protected override SettingUIBase CreateSettingUiBase(AppSettingDescription description)
        {
            if (Settings.Default[description.SettingName] == null)
            {
                throw new ArgumentException($"Passed setting name \"{description.SettingName}\" is not valid");
            }
            
            if (description.SettingType == "bool")
            {
                return new BooleanSettingUi(description);
            }

            if (description.SettingType == "int")
            {
                return new IntSettingUi(description);
            }

            if (description.SettingType == "string")
            {
                return new StringSettingUi(description);
            }

            throw new ArgumentException($"Passed setting type \"{description.SettingType}\" is not valid.");
        }
    }
}