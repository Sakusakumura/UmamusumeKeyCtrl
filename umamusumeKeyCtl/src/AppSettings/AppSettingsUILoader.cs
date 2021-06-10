using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using umamusumeKeyCtl.AppSettings.Factory;
using umamusumeKeyCtl.Properties;

namespace umamusumeKeyCtl.AppSettings
{
    public class AppSettingsUILoader
    {
        private StackPanel _appSettingsView;
        private AppSetting _appSetting;

        public AppSettingsUILoader(StackPanel AppSettingsView)
        {
            _appSettingsView = AppSettingsView;
            _appSetting = new AppSetting();
            _appSetting.AppSettingVersion = Version.Parse(Settings.Default.AppSettingVersion);
        }

        public async void LoadAndDraw()
        {
            try
            {
                if (!File.Exists("SettingDescriptions.json"))
                {
                    await Publish();
                    
                    LoadAndDraw();
                    
                    return;
                }

                var json = await File.ReadAllTextAsync("SettingDescriptions.json", Encoding.Unicode);
                
                var deserialized = (JsonSerializer.Deserialize<SerializedAppSetting>(json)).ToAppSetting();

                if (deserialized.AppSettingVersion < _appSetting.AppSettingVersion)
                {
                    await Publish();
                    
                    LoadAndDraw();
                    
                    return;
                }

                _appSetting = deserialized;

                foreach (var description in _appSetting.Descriptions)
                {
                    DrawDescription(description);
                }
            }
            catch (Exception e)
            {
                Debug.Write(e);
                throw;
            }
        }
        
        private async Task Publish()
        {
            _appSetting.AppSettingVersion = Version.Parse(Settings.Default.AppSettingVersion);
            
            var descriptions = _appSetting.Descriptions;
            descriptions.Add(new AppSettingDescription("AutoSave", "bool", "自動保存", "設定が追加・削除・編集されると自動で編集内容が保存されます。"));
            descriptions.Add(new AppSettingDescription("CaptureInterval", "int", "キャプチャ間隔", "ゲーム画面をキャプチャする間隔です。（ミリ秒）"));
            descriptions.Add(new AppSettingDescription("DetectorMethod", "int", "検出器メソッド", "検出器のメソッド。"));
            descriptions.Add(new AppSettingDescription("IsDebugMode", "bool", "デバッグモード", "デバッグ機能を有効にします。\nデバッグ機能使用中はCPUにより大きな負荷がかかります。"));

            var json = JsonSerializer.Serialize(_appSetting.ToSerializedAppSetting());

            try
            {
                await File.WriteAllTextAsync("SettingDescriptions.json", json, Encoding.Unicode);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                throw;
            }
        }

        private void DrawDescription(AppSettingDescription description)
        {
            var factory = new SettingUIFactory();
            var settingUiBase = factory.Create(description);
            var graphicalAppSetting = settingUiBase.GetSettingPanel();
            
            _appSettingsView.Children.Add(graphicalAppSetting.DockPanel);
        }
    }
}