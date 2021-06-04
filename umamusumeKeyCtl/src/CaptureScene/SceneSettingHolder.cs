using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using umamusumeKeyCtl.Util;

namespace umamusumeKeyCtl.CaptureScene
{
    public class SceneSettingHolder : Singleton<SceneSettingHolder>
    {
        private bool kill = false;

        private CancellationTokenSource _tokenSource;
        private Queue<Task> _taskQueue = new();
        
        private List<SceneSetting> _settings = new();
        public SceneSetting[] Settings => _settings.ToArray();

        public event Action<List<SceneSetting>> OnLoadSettings;

        public SceneSettingHolder()
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
                _settings = await Task<List<SceneSetting>>.Run(InternalAsyncLoadSettings);
                OnLoadSettings?.Invoke(_settings);
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                throw;
            }
        }

        private async Task<List<SceneSetting>> InternalAsyncLoadSettings()
        {
            List<SceneSetting> result = new();

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
                
                result = JsonSerializer.Deserialize<List<SceneSetting>>(str);
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
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
                Debug.Print(e.ToString());
                throw;
            }

            return Task.CompletedTask;
        }

        public void AddSettings(SceneSetting sceneSetting)
        {
            if (_settings.Contains(sceneSetting))
            {
                return;
            }
            
            _settings.Add(sceneSetting);
            
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
                    Debug.Print(e.ToString());
                    throw;
                }
            }
        }
    }
}