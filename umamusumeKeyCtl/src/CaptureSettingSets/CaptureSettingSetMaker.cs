using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using umamusumeKeyCtl.CaptureSettingSets.ImageScrapping;
using umamusumeKeyCtl.CaptureSettingSets.VirtualKeyPushing;
using umamusumeKeyCtl.Properties;

namespace umamusumeKeyCtl.CaptureSettingSets
{
    public class CaptureSettingSetMaker
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

        public event Action<CaptureSettingSet> OnCaptureSettingSetCreated; 

        private event Action<CaptureSettingMakeState> _onMakeStateChanged;

        private Canvas _canvas;
        private UIElement _uiElement;

        private string _name;
        private ScrapSetting _scrapSetting;
        private List<VirtualKeySetting> _virtualKeySettings;

        public CaptureSettingSetMaker(Canvas canvas, UIElement uiElement)
        {
            _uiElement = uiElement;
            _canvas = canvas;
            _onMakeStateChanged += OnStateChanged;

            SettingMakeState = CaptureSettingMakeState.Naming;

            OnCaptureSettingSetCreated += setting =>
            {
                CaptureSettingSetsHolder.Instance.AddSettings(setting);
                CaptureSettingSetsHolder.Instance.SaveSettings();
                CaptureSettingSetsHolder.Instance.LoadSettings();
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
                    using (bitmap)
                    {
                        try
                        {
                            Directory.CreateDirectory("./CapturedImages");
                            bitmap.PerformGrayScale().Save($"./CapturedImages/{_name}.bmp", ImageFormat.Bmp);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                        capture.StopCapture();
                    }
                });
                
                OnCaptureSettingSetCreated?.Invoke(new CaptureSettingSet(_name, _virtualKeySettings, _scrapSetting));
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