using System;
using System.Collections.Generic;
using System.Diagnostics;
using umamusumeKeyCtl.UserInput;

namespace umamusumeKeyCtl.CaptureScene
{
    public class Scene : IDisposable
    {
        public SceneSetting Setting { get; }
        
        public ScrappedImage ScrappedImage { get; }
        public List<VirtualKey> VirtualKeys { get; }

        private LowLevelKeyboardListener _keyboardListener;

        private bool _isSelected = false;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                VirtualKeys.ForEach(val => val.Initialize());
            }
        }

        public Scene(SceneSetting setting, ScrappedImage scrappedImage, List<VirtualKey> virtualKeys, LowLevelKeyboardListener listener)
        {
            Setting = setting;
            ScrappedImage = scrappedImage;
            VirtualKeys = virtualKeys;

            _keyboardListener = listener;
            _keyboardListener.OnKeyPressed += OnKeyPressed;
        }

        public void SetWindowHandle(IntPtr hwnd)
        {
            foreach (var virtualKey in VirtualKeys)
            {
                virtualKey.UmaWndH = hwnd;
            }
        }

        private void OnKeyPressed(object sender, KeyPressedArgs e)
        {
            if (IsSelected == false)
            {
                return;
            }
            
            var vKey = VirtualKeys.Find(val => val.Setting.BindKey == e.KeyPressed);

            if (vKey != null)
            {
                vKey.Perform(e);
            }
        }

        public void Dispose()
        {
            ScrappedImage?.Dispose();
            
            if (_keyboardListener != null)
            {
                _keyboardListener.OnKeyPressed -= OnKeyPressed;
                _keyboardListener = null;
            }
        }
    }
}