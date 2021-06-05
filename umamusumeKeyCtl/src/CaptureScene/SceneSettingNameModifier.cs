using System;

namespace umamusumeKeyCtl.CaptureScene
{
    public class SceneSettingNameModifier
    {
        public event Action<string> CompleteInputName;
        
        private ModifyState _state = ModifyState.Waiting;
        private NameInputPopupWindow _window;
        
        public void OnModifyTitleClicked()
        {
            if (_state != ModifyState.Waiting)
            {
                return;
            }

            _window = new NameInputPopupWindow();
            _window.OnConfirm += NameInputPopupWindowOnOnConfirm;
            _window.OnCanceled += NameInputPopupWindowOnOnCanceled;
            _window.ShowDialog();

            _state = ModifyState.Naming;
        }

        private void NameInputPopupWindowOnOnCanceled()
        {
            _window.OnConfirm -= NameInputPopupWindowOnOnConfirm;
            _window.OnCanceled -= NameInputPopupWindowOnOnCanceled;
            _window = null;

            _state = ModifyState.Waiting;
        }

        private void NameInputPopupWindowOnOnConfirm(string obj)
        {
            CompleteInputName?.Invoke(obj);

            _state = ModifyState.Waiting;
        }

        private enum ModifyState
        {
            Waiting,
            Naming
        }
    }
}