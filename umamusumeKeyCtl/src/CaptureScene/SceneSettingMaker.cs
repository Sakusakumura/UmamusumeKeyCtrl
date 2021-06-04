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
                var nameInWnd = new NameInputPopupWindow();
                nameInWnd.OnConfirm += OnGetName;
                nameInWnd.ShowDialog();
                
                return;
            }

            if (state == CaptureSettingMakeState.ScrapSetting)
            {
                var scrapSettingMaker = new ScrapSettingMaker(_canvas, _uiElement, true);
                scrapSettingMaker.OnMadeScrapSetting += OnGetScrapSetting;
                
                return;
            }

            if (state == CaptureSettingMakeState.VirtualKeySetting)
            {
                var virtualKeySettingMaker = new VirtualKeySettingMaker(_canvas, _uiElement, true);
                virtualKeySettingMaker.OnSettingCreated += OnGetVirtualKeySetting;
            }

            if (state == CaptureSettingMakeState.Completed)
            {
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
                                    using (var grayScaled = bitmap.PerformGrayScale())
                                    {
                                        grayScaled.Save($"./CapturedImages/{_name}.bmp", ImageFormat.Bmp);
                                    }
                                }

                                OnCaptureSettingSetCreated?.Invoke(new SceneSetting(_name, _virtualKeySettings, _scrapSetting));
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

        private void OnGetName(string str)
        {
            _name = str;
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