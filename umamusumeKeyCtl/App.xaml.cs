using System.Windows;
using umamusumeKeyCtl.CaptureScene;

namespace umamusumeKeyCtl
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            SceneSettingHolder.Instance.Kill();
        }
    }
}