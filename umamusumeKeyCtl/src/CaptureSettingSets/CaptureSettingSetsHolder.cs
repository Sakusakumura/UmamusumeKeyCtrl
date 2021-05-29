using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using umamusumeKeyCtl.Util;

namespace umamusumeKeyCtl.CaptureSettingSets
{
    public class CaptureSettingSetsHolder : Singleton<CaptureSettingSetsHolder>
    {
        private bool kill = false;

        private CancellationTokenSource _tokenSource;
        private Queue<Task> _taskQueue = new();
        
        private List<CaptureSettingSet> _settings = new();
        public CaptureSettingSet[] Settings => _settings.ToArray();

        public event Action<List<CaptureSettingSet>> OnLoadSettings;

        public CaptureSettingSetsHolder()
        {
            _tokenSource = new CancellationTokenSource();
            _ = ExecuteQueue(_tokenSource.Token);
        }

        public void Kill()
        {
            kill = true;
        }

        public void LoadSettings()
        {
            _taskQueue.Enqueue(AsyncLoadSettings());
        }

        private async Task AsyncLoadSettings()
        {
            try
            {
                _settings = await Task<List<CaptureSettingSet>>.Run(InternalAsyncLoadSettings);
                OnLoadSettings?.Invoke(_settings);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task<List<CaptureSettingSet>> InternalAsyncLoadSettings()
        {
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
                Console.WriteLine(e);
                throw;
            }

            return result;
        }

        public void SaveSettings()
        {
            _taskQueue.Enqueue(InternalSaveSettings());
        }

        private Task InternalSaveSettings()
        {
            try
            {
                var str = JsonSerializer.Serialize(Settings);
                File.WriteAllText("ScrapSettings.txt", str, Encoding.Unicode);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return Task.CompletedTask;
        }

        public void AddSettings(CaptureSettingSet settingSet)
        {
            if (_settings.Contains(settingSet))
            {
                return;
            }
            
            _settings.Add(settingSet);
            
            OnLoadSettings?.Invoke(_settings);

            if (Properties.Settings.Default.AutoSave)
            {
                SaveSettings();
            }
        }

        public void RemoveSetting(string settingName)
        {
            var target = _settings.Find(setting => setting.Name == settingName);
            
            if (target == null)
            {
                return;
            }

            _settings.Remove(target);
            
            OnLoadSettings?.Invoke(_settings);

            if (Properties.Settings.Default.AutoSave)
            {
                SaveSettings();
            }
        }

        private async Task ExecuteQueue(CancellationToken token)
        {
            while (token.IsCancellationRequested == false && kill == false)
            {
                while (_taskQueue.Count == 0 && token.IsCancellationRequested == false && kill == false)
                {
                    await Task.Delay(1);
                }

                try
                {
                    var task = _taskQueue.Dequeue();
                    
                    await Task.Run(() => task, token);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            
            Debug.Print($"Execute queue finished. Statuses: kill={kill}, cancellationToken.IsCancellationRequested={token.IsCancellationRequested}");
        }
    }
}