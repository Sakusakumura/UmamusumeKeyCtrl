using System;
using System.Collections.Generic;
using System.Drawing;
using umamusumeKeyCtl.UserInput;

namespace umamusumeKeyCtl.CaptureScene
{
    public class Scene : IDisposable
    {
        public SceneSetting Setting { get; }
        
        public ScrappedImage ScrappedImage { get; }
        public List<VirtualKey> VirtualKeys { get; }

        private LowLevelKeyboardListener _keyboardListener;
        public void Hook() => _keyboardListener?.HookKeyboard();
        public void UnHook() => _keyboardListener?.UnHookKeyboard();

        public Scene(SceneSetting setting, ScrappedImage scrappedImage, List<VirtualKey> virtualKeys)
        {
            Setting = setting;
            ScrappedImage = scrappedImage;
            VirtualKeys = virtualKeys;
            
            Initialize();
        }

        public void SetWindowHandle(IntPtr hwnd)
        {
            foreach (var virtualKey in VirtualKeys)
            {
                virtualKey.UmaWndH = hwnd;
            }
        }

        /// <summary>
        /// TODO: Compute with captured image and ScrappedImage.
        /// </summary>
        /// <param name="capturedImage"></param>
        public void Compute(Bitmap capturedImage)
        {
            using (capturedImage)
            {
                
            }
        }

        private void Initialize()
        {
            _keyboardListener = new LowLevelKeyboardListener();
            _keyboardListener.OnKeyPressed += OnKeyPressed;
        }

        private void OnKeyPressed(object sender, KeyPressedArgs e)
        {
            var vKey = VirtualKeys.Find(val => val.Setting.BindKey == e.KeyPressed);

            if (vKey != null)
            {
                vKey.Push();
            }
        }

        public void Dispose()
        {
            ScrappedImage?.Dispose();
            _keyboardListener?.UnHookKeyboard();
            _keyboardListener = null;
        }
    }
}