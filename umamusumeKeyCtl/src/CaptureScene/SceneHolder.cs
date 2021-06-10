using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using umamusumeKeyCtl.ImageSimilarity.Factory;
using umamusumeKeyCtl.ImageSimilarity.Method;
using umamusumeKeyCtl.Properties;
using umamusumeKeyCtl.UserInput;
using umamusumeKeyCtl.Util;

namespace umamusumeKeyCtl.CaptureScene
{
    public class SceneHolder : Singleton<SceneHolder>, IDisposable
    {
        private List<Scene> _scenes;
        public List<Scene> Scenes => _scenes;

        public event Action<List<Scene>> OnLoadScenes;
        private event Action<IntPtr> OnGetUmaWndh;
        private IntPtr umaWndh = IntPtr.Zero;

        private CancellationTokenSource _cancellationTokenSource;
        private LowLevelKeyboardListener _lowLevelKeyboardListener;

        public SceneHolder()
        {
            _lowLevelKeyboardListener = new LowLevelKeyboardListener();
            _lowLevelKeyboardListener.HookKeyboard();
            
            _scenes = new List<Scene>();
            SceneSettingHolder.Instance.OnLoadSettings += OnLoadSettings;

            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => { return AsyncWindowHandleCheck(_cancellationTokenSource.Token); }, _cancellationTokenSource.Token);
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            
            if (_scenes?.Count > 0)
            {
                _scenes?.ForEach(scene => scene?.Dispose());
            }
        }

        private void OnLoadSettings(List<SceneSetting> sceneSettings)
        {
            var toBeCreated = new List<SceneSetting>();
            
            // Create if setting is added
            foreach (var sceneSetting in sceneSettings)
            {
                if (_scenes.Exists(val => val.Setting.Guid == sceneSetting.Guid))
                {
                    continue;
                }
                
                toBeCreated.Add(sceneSetting);
            }

            // Remove instance if setting is removed
            foreach (var guid in _scenes.ToArray().Select(val => val.Setting.Guid))
            {
                if (sceneSettings.Exists(val => val.Guid == guid))
                {
                    continue;
                }
                
                var target = _scenes.Find(val => val.Setting.Guid == guid);
                _scenes.Remove(target);
                Debug.Print($"[SceneHolder] {target.Setting.DisplayName}(Guid: {target.Setting.Guid}) removed.");
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
                
                Debug.Print($"[SceneHolder] {instance.Setting.DisplayName}(Guid: {instance.Setting.Guid}) instantiated.");
            }
            
            OnLoadScenes?.Invoke(_scenes);
        }

        private Scene InternalCreateScene(SceneSetting setting)
        {
            Scene instance;
            
            var sourcePath = $"{Settings.Default.ScreenShotLocation}/{setting.Guid}.bmp";

            try
            {
                using (var source = new Bitmap(sourcePath))
                {
                    var scrappedImage = new ScrappedImage((Bitmap) source.Clone(), setting.ScrapSetting, setting.DetectorMethod, setting.DescriptorMethod);

                    var vkList = new List<VirtualKey>();
                    foreach (var virtualKeySetting in setting.VirtualKeySettings)
                    {
                        vkList.Add(new VirtualKey(virtualKeySetting));
                    }

                    instance = new Scene(setting, scrappedImage, vkList, _lowLevelKeyboardListener);

                    if (umaWndh != IntPtr.Zero)
                    {
                        instance.SetWindowHandle(umaWndh);
                    }

                    OnGetUmaWndh += instance.SetWindowHandle;
                }
            }
            catch (Exception e)
            {
                Debug.Write(e);
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
                if (IsWindow(umaWndh) )
                {
                    await Task.Delay(500);
                    continue;
                }
                
                umaWndh = WindowHelper.GetHWndByName(Settings.Default.CaptureWindowTitle);
                OnGetUmaWndh?.Invoke(umaWndh);
            }
        }
    }
}