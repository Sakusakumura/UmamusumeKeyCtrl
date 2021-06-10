using System;
using umamusumeKeyCtl.ImageSimilarity.Factory;

namespace umamusumeKeyCtl.CaptureScene
{
    public class SceneSettingNameModifier
    {
        public event EventHandler<Tuple<string, DetectorMethod, DescriptorMethod>> CompleteInputName;
        
        private ModifyState _state = ModifyState.Waiting;
        private NameInputPopupWindow _window;
        private string _displayName;
        private int _detectorMethod;
        private int _descriptorMethod;

        public SceneSettingNameModifier(SceneSetting sceneSetting)
        {
            _displayName = sceneSetting.DisplayName;
            _detectorMethod = (int) sceneSetting.DetectorMethod;
            _descriptorMethod = (int) sceneSetting.DescriptorMethod;
        }

        public void OnModifyTitleClicked()
        {
            if (_state != ModifyState.Waiting)
            {
                return;
            }

            _state = ModifyState.Naming;
            
            _window = new NameInputPopupWindow(false, _displayName, _detectorMethod, _descriptorMethod);
            _window.Confirm += NameInputPopupWindowOnConfirm;
            _window.Canceled += NameInputPopupWindowOnCanceled;
            _window.ShowDialog();
        }

        private void NameInputPopupWindowOnCanceled(object sender, EventArgs eventArgs)
        {
            _state = ModifyState.Waiting;
            
            _window.Confirm -= NameInputPopupWindowOnConfirm;
            _window.Canceled -= NameInputPopupWindowOnCanceled;
            _window = null;
        }

        private void NameInputPopupWindowOnConfirm(object sender, Tuple<string, DetectorMethod, DescriptorMethod> tuple)
        {
            _state = ModifyState.Waiting;

            _displayName = tuple.Item1;
            _detectorMethod = (int) tuple.Item2;
            _descriptorMethod = (int) tuple.Item3;
            
            CompleteInputName?.Invoke(this, tuple);
        }

        private enum ModifyState
        {
            Waiting,
            Naming
        }
    }
}