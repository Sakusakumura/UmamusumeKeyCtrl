using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using umamusumeKeyCtl.AppSettings.Factory;

namespace umamusumeKeyCtl.AppSettings
{
    public class AppSettingsUILoader
    {
        private StackPanel _appSettingsView;
        private List<AppSettingDescription> _descriptions;

        public AppSettingsUILoader(StackPanel AppSettingsView)
        {
            _appSettingsView = AppSettingsView;
            _descriptions = new List<AppSettingDescription>();
        }

        public async void LoadAndDraw()
        {
            try
            {
                if (!File.Exists("SettingDescriptions"))
                {
                    await Publish();
                    
                    LoadAndDraw();
                    
                    return;
                }

                var readTask = await Task<byte[]>.Run(() => File.ReadAllBytes("SettingDescriptions"));
                
                var json = Encoding.Unicode.GetString(readTask);
                
                _descriptions = JsonSerializer.Deserialize<List<AppSettingDescription>>(json);

                foreach (var description in _descriptions)
                {
                    DrawDescription(description);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        private async Task Publish()
        {
            _descriptions.Add(new AppSettingDescription("AutoSave", "bool", "自動保存", "設定が追加・削除・編集されると自動で編集内容が保存されます。"));
            _descriptions.Add(new AppSettingDescription("CaptureSetting", "int", "キャプチャ間隔", "ゲーム画面をキャプチャする間隔です。（ミリ秒）"));

            var json = JsonSerializer.Serialize(_descriptions);

            try
            {
                await Task.Run(() => File.WriteAllBytes("SettingDescriptions", Encoding.Unicode.GetBytes(json)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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