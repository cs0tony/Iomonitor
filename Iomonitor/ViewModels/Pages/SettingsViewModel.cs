using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace Iomonitor.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<string> _languageList = new ObservableCollection<string>()
        {
            "en-US",
            "zh-CN"
        };

        [ObservableProperty]
        private bool _runAtStartup = RunAtStartupSetter.IsRunAtStartup();

        [RelayCommand]
        private void RunAtStartupToggle()
        {
            if (RunAtStartup)
            {
                RunAtStartupSetter.RunAtStartupEnable();
            }
            else
            {
                RunAtStartupSetter.RunAtStartupDisable();
            }
        }

        private void RunAtStartupToggleInRegistryKey()
        {
            string appname = Assembly.GetEntryAssembly().GetName().Name;
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (RunAtStartup)
                {
                    string executablePath = Process.GetCurrentProcess().MainModule.FileName;
                    rk.SetValue(appname, executablePath);
                }
                else
                {
                    rk.DeleteValue(appname, false);
                }
            }
        }

        [ObservableProperty]
        private bool _themeChangeEnabled = true;

        [RelayCommand]
        private void SetMonitorInternal()
        {
            Utils.ChangeDisplayMode("/internal"); // 仅显示internal模式
        }

        [RelayCommand]
        private void SetMonitorClone()
        {
            Utils.ChangeDisplayMode("/clone"); // 复制模式
        }

        [RelayCommand]
        private void SetMonitorExternal()
        {
            Utils.ChangeDisplayMode("/external"); // 仅第二屏幕模式
        }

        [RelayCommand]
        private void SetMonitorExtend()
        {
            Utils.ChangeDisplayMode("/extend"); // 扩展模式
        }
    }
}
