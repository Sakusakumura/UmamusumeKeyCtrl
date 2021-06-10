using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using umamusumeKeyCtl.ImageSimilarity.Factory;
using umamusumeKeyCtl.Properties;

namespace umamusumeKeyCtl.CaptureScene
{
    public class SceneSettingMaker
    {
        private CaptureSettingMakeState _settingMakeState = CaptureSettingMakeState.Waiting;

        private CaptureSettingMakeState SettingMakeState
        {
            get => _settingMakeState;
            set
            {
                _settingMakeState = value;
                _onMakeStateChanged?.Invoke(_settingMakeState);
            }
        }

        public event Action<SceneSetting> OnCaptureSettingSetCreated; 

        private event Action<CaptureSettingMakeState> _onMakeStateChanged;

        private Canvas _canvas;
        private UIElement _uiElement;

        private string _name;
        private DetectorMethod _detectorMethod;
        private DescriptorMethod _descriptorMethod;
        private ScrapSetting _scrapSetting;
        private List<VirtualKeySetting> _virtualKeySettings;

        public SceneSettingMaker(Canvas canvas, UIElement uiElement)
        {
            _uiElement = uiElement;
            _canvas = canvas;
            _onMakeStateChanged += OnStateChanged;

            SettingMakeState = CaptureSettingMakeState.Naming;

            OnCaptureSettingSetCreated += setting =>
            {
                SceneSettingHolder.Instance.AddSettings(setting);
                SceneSettingHolder.Instance.SaveSettings();
                SceneSettingHolder.Instance.LoadSettings();
            };
        }

        private void OnStateChanged(CaptureSettingMakeState state)
        {
            if (state == CaptureSettingMakeState.Naming)
            {
                var nameInWnd = new NameInputPopupWindow(true);
                nameInWnd.Confirm += OnConfirm;
                nameInWnd.ShowDialog();
                
                return;
            }

            if (state == CaptureSettingMakeState.ScrapSetting)
            {
                var scrapSettingMaker = new ScrapSettingMaker(_canvas, _uiElement, true);
                scrapSettingMaker.MadeScrapSetting += OnGetScrapSetting;
                
                return;
            }

            if (state == CaptureSettingMakeState.VirtualKeySetting)
            {
                var virtualKeySettingMaker = new VirtualKeySettingMaker(_canvas, _uiElement, true);
                virtualKeySettingMaker.SettingCreated += OnGetVirtualKeySetting;
            }

            if (state == CaptureSettingMakeState.Completed)
            {
                var sceneSetting = new SceneSetting(Guid.NewGuid(), _name, _virtualKeySettings, _scrapSetting, _detectorMethod, _descriptorMethod);

                // take a screenshot.
                var capture = new WindowCapture(new CaptureSetting(Settings.Default.CaptureInterval, Settings.Default.CaptureWindowTitle));
                capture.CaptureResultObservable.Subscribe(bitmap =>
                {
                    try
                    {
                        Directory.CreateDirectory("./CapturedImages");
                        Task.Run(() =>
                        {
                            try
                            {
                                using (bitmap)
                                {
                                    bitmap.Save($"./CapturedImages/{sceneSetting.Guid}.bmp", ImageFormat.Bmp);
                                }

                                OnCaptureSettingSetCreated?.Invoke(sceneSetting);
                            }
                            catch (Exception e)
                            {
                                Debug.Print(e.ToString());
                                throw;
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        Debug.Print(e.ToString());
                        throw;
                    }
                            
                    capture.StopCapture();

                    capture.Dispose();
                });
            }
        }

        private void OnCancel()
        {
            _settingMakeState = CaptureSettingMakeState.Waiting;
        }

        private void OnConfirm(object sender, Tuple<string, DetectorMethod, DescriptorMethod> tuple)
        {
            _name = tuple.Item1;
            _detectorMethod = tuple.Item2;
            _descriptorMethod = tuple.Item3;
            SettingMakeState = CaptureSettingMakeState.ScrapSetting;
        }

        private void OnGetScrapSetting(ScrapSetting setting)
        {
            _scrapSetting = setting;
            SettingMakeState = CaptureSettingMakeState.VirtualKeySetting;
        }

        private void OnGetVirtualKeySetting(List<VirtualKeySetting> setting)
        {
            _virtualKeySettings = setting;
            SettingMakeState = CaptureSettingMakeState.Completed;
        }

        private enum CaptureSettingMakeState
        {
            Waiting,
            Naming,
            ScrapSetting,
            VirtualKeySetting,
            Completed
        }
    }
}