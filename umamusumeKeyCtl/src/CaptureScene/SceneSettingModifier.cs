using System.Collections.Generic;
using System.Windows.Controls;

namespace umamusumeKeyCtl.CaptureScene
{
    public class SceneSettingModifier
    {
        private SceneSetting _sceneSetting;
        private SceneSettingModifyToolBox _toolBox;
        private ScrapSettingModifier _scrapSettingModifier;
        private VirtualKeySettingModifier _virtualKeySettingModifier;
        private Canvas _canvas;

        public SceneSettingModifier(SceneSetting sceneSetting, Canvas drawToCanvas, StackPanel toolPanel)
        {
            _toolBox = new SceneSettingModifyToolBox(toolPanel);
            _toolBox.OnFinishEditing += OnFinishEditing;

            _canvas = drawToCanvas;
            
            _sceneSetting = sceneSetting;
            DrawSettingToCanvas(_sceneSetting, _canvas);
        }

        public void Discard()
        {
            _scrapSettingModifier.Discard();
            _virtualKeySettingModifier.Discard();
            _toolBox.Discard();
        }

        private void OnFinishEditing()
        {
            var instance = SceneSettingHolder.Instance;
            instance.RemoveSetting(_sceneSetting.Name);
            instance.AddSettings(_sceneSetting);
            
            Discard();
        }

        private void DrawSettingToCanvas(SceneSetting target, Canvas drawToCanvas)
        {
            _scrapSettingModifier = new ScrapSettingModifier(target.ScrapSetting, drawToCanvas);
            _scrapSettingModifier.OnChangeScrapSetting += OnChangeScrapSetting;
            _toolBox.OnScrapSettingModifyModeSelected += _scrapSettingModifier.OnEditModeChanged;
            
            _virtualKeySettingModifier = new VirtualKeySettingModifier(target.VirtualKeySettings, drawToCanvas);
            _virtualKeySettingModifier.OnChangeVirtualKeys += OnChangeVirtualKeys;
            _toolBox.OnVirtualKeySettingModifyModeSelected += _virtualKeySettingModifier.OnEditModeChanged;
        }

        private void Repaint()
        {
            if (_scrapSettingModifier == null || _virtualKeySettingModifier == null)
            {
                return;
            }
            
            _scrapSettingModifier.Repaint();
            _virtualKeySettingModifier.Repaint();
        }

        private void OnChangeScrapSetting(ScrapSetting setting)
        {
            _sceneSetting =
                new SceneSetting(_sceneSetting.Name, _sceneSetting.VirtualKeySettings, setting);

            _toolBox.ScrapSettingEditMode = _toolBox.ScrapSettingEditMode == EditMode.Add ? EditMode.Modify : _toolBox.ScrapSettingEditMode;

            Repaint();
        }

        private void OnChangeVirtualKeys(List<VirtualKeySetting> virtualKeys)
        {
            _sceneSetting =
                new SceneSetting(_sceneSetting.Name, virtualKeys, _sceneSetting.ScrapSetting);
            
            _toolBox.VirtualKeySettingEditMode = _toolBox.VirtualKeySettingEditMode == EditMode.Add ? EditMode.Modify : _toolBox.VirtualKeySettingEditMode;

            Repaint();
        }
    }
}