using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using umamusumeKeyCtl.Properties;
using umamusumeKeyCtl.Util;

namespace umamusumeKeyCtl.CaptureScene
{
    public class SceneHolder : Singleton<SceneHolder>, IDisposable
    {
        private List<Scene> _scenes;
        public Scene[] Scenes => _scenes.ToArray();

        public event Action<Scene[]> OnLoadScenes;
        private event Action<IntPtr> OnGetUmaWndh;
        private IntPtr umaWndh = IntPtr.Zero;

        private CancellationTokenSource _cancellationTokenSource;

        public SceneHolder()
        {
            _scenes = new List<Scene>();
            SceneSettingHolder.Instance.OnLoadSettings += OnLoadSettings;

            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => { return AsyncWindowHandleCheck(_cancellationTokenSource.Token); }, _cancellationTokenSource.Token);
        }

        private void OnLoadSettings(List<SceneSetting> sceneSettings)
        {
            var toBeCreated = new List<SceneSetting>();
            
            // Create if setting is added
            foreach (var sceneSetting in sceneSettings)
            {
                if (_scenes.Exists(val => val.Setting.Name == sceneSetting.Name))
                {
                    continue;
                }
                
                toBeCreated.Add(sceneSetting);
            }

            // Remove instance if setting is removed
            foreach (var name in _scenes.ToArray().Select(val => val.Setting.Name))
            {
                if (sceneSettings.Exists(val => val.Name == name))
                {
                    continue;
                }
                
                var target = _scenes.Find(val => val.Setting.Name == name);
                _scenes.Remove(target);
                Debug.Print($"[SceneHolder] {target.Setting.Name} removed.");
                target.Dispose();
            }

            if (toBeCreated.Count > 0)
            {
                Task.Run(() => AsyncCreateInstances(toBeCreated));
            }
        }

        private void AsyncCreateInstances(List<SceneSetting> settings)
        {
            foreach (var setting in settings)
            {
                var instance = InternalCreateScene(setting);
                _scenes.Add(instance);
                
                Debug.Print($"[SceneHolder] {instance.Setting.Name} instantiated.");
            }
            
            OnLoadScenes?.Invoke(_scenes.ToArray());
        }

        private Scene InternalCreateScene(SceneSetting setting)
        {
            Scene instance;
            
            var sourcePath = $"{Settings.Default.ScreenShotLocation}/{setting.Name}.bmp";

            try
            {
                using (var source = new Bitmap(sourcePath))
                {
                    var scrappedImage = new ScrappedImage((Bitmap) source.Clone(), setting.ScrapSetting);

                    var vkList = new List<VirtualKey>();
                    foreach (var virtualKeySetting in setting.VirtualKeySettings)
                    {
                        vkList.Add(new VirtualKey(virtualKeySetting));
                    }

                    instance = new Scene(setting, scrappedImage, vkList);

                    OnGetUmaWndh += instance.SetWindowHandle;
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                throw;
            }

            return instance;
        }
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);

        private async Task AsyncWindowHandleCheck(CancellationToken token)
        {
            while (token.IsCancellationRequested == false)
            {
                if (IsWindow(umaWndh))
                {
                    await Task.Delay(500);
                    continue;
                }
                
                umaWndh = WindowHelper.GetHWndByName(Settings.Default.CaptureWindowTitle);
                OnGetUmaWndh.Invoke(umaWndh);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
            
            if (_scenes?.Count > 0)
            {
                _scenes?.ForEach(scene => scene?.Dispose());
            }
        }
    }
}