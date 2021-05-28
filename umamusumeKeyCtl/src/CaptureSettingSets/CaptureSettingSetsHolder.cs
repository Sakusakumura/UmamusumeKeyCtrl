using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace umamusumeKeyCtl.CaptureSettingSets
{
    public class CaptureSettingSetsHolder : Singleton<CaptureSettingSetsHolder>
    {
        private bool isBusy = false;
        
        private List<CaptureSettingSet> _settings = new();
        public List<CaptureSettingSet> Settings
        {
            get => _settings;
            private set
            {
                _settings = value;
                OnLoadSettings.Invoke(_settings);
            }
        }

        public event Action<List<CaptureSettingSet>> OnLoadSettings;

        public async void LoadSettings()
        {
            if (isBusy)
            {
                return;
            }
            
            try
            {
                Settings = await Task<List<CaptureSettingSet>>.Run(AsyncLoadSettings);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task<List<CaptureSettingSet>> AsyncLoadSettings()
        {
            isBusy = true;
            List<CaptureSettingSet> result = new();

            try
            {
                if (!File.Exists("ScrapSettings.txt"))
                {
                    await Task.Run(() => File.Create("ScrapSettings.txt"));
                }
            
                var str = File.ReadAllText("ScrapSettings.txt");

                if (String.IsNullOrEmpty(str))
                {
                    return result;
                }
                
                result = JsonSerializer.Deserialize<List<CaptureSettingSet>>(str);
            }
            catch (Exception e)
            {
                isBusy = false;
                Console.WriteLine(e);
                throw;
            }

            isBusy = false;

            return result;
        }

        public void SaveSettings()
        {
            if (isBusy)
            {
                return;
            }
            
            try
            {
                _ = Task.Run(AsyncSaveSettings);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task AsyncSaveSettings()
        {
            isBusy = true;
            
            try
            {
                var str = JsonSerializer.Serialize(Settings);
                File.WriteAllText("ScrapSettings.txt", str, Encoding.Unicode);
            }
            catch (Exception e)
            {
                isBusy = false;
                Console.WriteLine(e);
                throw;
            }

            isBusy = false;
        }

        public void AddSettings(CaptureSettingSet settingSet)
        {
            if (Settings.Contains(settingSet))
            {
                return;
            }
            
            Settings.Add(settingSet);
        }

        public void RemoveSetting(string settingName)
        {
            var target = Settings.Find(setting => setting.Name == settingName);
            
            if (target == null)
            {
                return;
            }

            Settings.Remove(target);
            
            OnLoadSettings.Invoke(Settings);
        }
    }
}