using System;
using umamusumeKeyCtl.ImageSimilarity.Factory;

namespace umamusumeKeyCtl.CaptureScene
{
    public class SceneSettingNameModifier
    {
        public event EventHandler<Tuple<string, DetectorMethod, DescriptorMethod>> CompleteInputName;
        
        private ModifyState _state = ModifyState.Waiting;
        private NameInputPopupWindow _window;
        private SceneSetting _sceneSetting;

        public SceneSettingNameModifier(SceneSetting sceneSetting)
        {
            _sceneSetting = sceneSetting;
        }

        public void OnModifyTitleClicked()
        {
            if (_state != ModifyState.Waiting)
            {
                return;
            }

            _window = new NameInputPopupWindow(false, _sceneSetting.DisplayName, (int) _sceneSetting.DetectorMethod, (int) _sceneSetting.DescriptorMethod);
            _window.Confirm += NameInputPopupWindowOnConfirm;
            _window.Canceled += NameInputPopupWindowOnCanceled;
            _window.ShowDialog();

            _state = ModifyState.Naming;
        }

        private void NameInputPopupWindowOnCanceled(object sender, EventArgs eventArgs)
        {
            _window.Confirm -= NameInputPopupWindowOnConfirm;
            _window.Canceled -= NameInputPopupWindowOnCanceled;
            _window = null;

            _state = ModifyState.Waiting;
        }

        private void NameInputPopupWindowOnConfirm(object sender, Tuple<string, DetectorMethod, DescriptorMethod> tuple)
        {
            CompleteInputName?.Invoke(this, tuple);

            _state = ModifyState.Waiting;
        }

        private enum ModifyState
        {
            Waiting,
            Naming
        }
    }
}