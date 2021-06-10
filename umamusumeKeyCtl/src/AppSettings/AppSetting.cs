using System;
using System.Collections.Generic;

namespace umamusumeKeyCtl.AppSettings
{
    public class AppSetting
    {
        public Version AppSettingVersion { get; set; }
        public List<AppSettingDescription> Descriptions { get; set; }

        public AppSetting()
        {
            AppSettingVersion = new Version();
            Descriptions = new List<AppSettingDescription>();
        }

        public AppSetting(Version appSettingVersion, List<AppSettingDescription> descriptions)
        {
            AppSettingVersion = appSettingVersion;
            Descriptions = descriptions;
        }

        public SerializedAppSetting ToSerializedAppSetting()
        {
            return new SerializedAppSetting(AppSettingVersion.ToString(), Descriptions);
        }
    }

    public class SerializedAppSetting
    {
        public string AppSettingVersion { get; set; }
        public List<AppSettingDescription> Descriptions { get; set; }

        public SerializedAppSetting()
        {
        }

        public SerializedAppSetting(string appSettingVersion, List<AppSettingDescription> descriptions)
        {
            AppSettingVersion = appSettingVersion;
            Descriptions = descriptions;
        }

        public AppSetting ToAppSetting()
        {
            return new AppSetting(Version.Parse(this.AppSettingVersion), Descriptions);
        }
    }
}