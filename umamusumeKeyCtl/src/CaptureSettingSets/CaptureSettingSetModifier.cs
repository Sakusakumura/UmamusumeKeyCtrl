using System.Collections.Generic;
using System.Windows.Controls;
using umamusumeKeyCtl.CaptureSettingSets.ImageScrapping;
using umamusumeKeyCtl.CaptureSettingSets.VirtualKeyPushing;

namespace umamusumeKeyCtl.CaptureSettingSets
{
    public class CaptureSettingSetModifier
    {
        private CaptureSettingSet _captureSettingSet;
        private CaptureSettingSetModifyToolBox _toolBox;
        private ScrapSettingModifier _scrapSettingModifier;
        private VirtualKeySettingModifier _virtualKeySettingModifier;
        private Canvas _canvas;

        public CaptureSettingSetModifier(CaptureSettingSet captureSettingSet, Canvas drawToCanvas, StackPanel toolPanel)
        {
            _toolBox = new CaptureSettingSetModifyToolBox(toolPanel);
            _toolBox.OnFinishEditing += OnFinishEditing;

            _canvas = drawToCanvas;
            
            _captureSettingSet = captureSettingSet;
            DrawSettingToCanvas(_captureSettingSet, _canvas);
        }

        public void Discard()
        {
            _scrapSettingModifier.Discard();
            _virtualKeySettingModifier.Discard();
            _toolBox.Discard();
        }

        private void OnFinishEditing()
        {
            var instance = CaptureSettingSetsHolder.Instance;
            instance.RemoveSetting(_captureSettingSet.Name);
            instance.AddSettings(_captureSettingSet);
            
            Discard();
        }

        private void DrawSettingToCanvas(CaptureSettingSet target, Canvas drawToCanvas)
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
            _captureSettingSet =
                new CaptureSettingSet(_captureSettingSet.Name, _captureSettingSet.VirtualKeySettings, setting);

            _toolBox.ScrapSettingEditMode = _toolBox.ScrapSettingEditMode == EditMode.Add ? EditMode.Modify : _toolBox.ScrapSettingEditMode;

            Repaint();
        }

        private void OnChangeVirtualKeys(List<VirtualKeySetting> virtualKeys)
        {
            _captureSettingSet =
                new CaptureSettingSet(_captureSettingSet.Name, virtualKeys, _captureSettingSet.ScrapSetting);
            
            _toolBox.VirtualKeySettingEditMode = _toolBox.VirtualKeySettingEditMode == EditMode.Add ? EditMode.Modify : _toolBox.VirtualKeySettingEditMode;

            Repaint();
        }
    }
}