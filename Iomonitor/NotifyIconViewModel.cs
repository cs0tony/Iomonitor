using Wpf.Ui;

namespace Iomonitor
{
    public partial class NotifyIconViewModel : ObservableObject
    {
        [RelayCommand]
        public void ShowWindow()
        {
            INavigationWindow _navigationWindow = App.GetService<INavigationWindow>();
            _navigationWindow!.ShowWindow();
            // 检查窗口是否最小化
            if ((_navigationWindow as Window)!.WindowState == WindowState.Minimized)
            {
                // 取消最小化状态
                (_navigationWindow as Window)!.WindowState = WindowState.Normal;
            }
            (_navigationWindow as Window)!.Activate();
        }

        [RelayCommand]
        private void SetMonitorInternal()
        {
            Utils.ChangeDisplayMode("/internal");
        }

        [RelayCommand]
        private void SetMonitorClone()
        {
            Utils.ChangeDisplayMode("/clone");
        }

        [RelayCommand]
        private void SetMonitorExtend()
        {
            Utils.ChangeDisplayMode("/extend");
        }

        [RelayCommand]
        private void SetMonitorExternal()
        {
            Utils.ChangeDisplayMode("/external");
        }

        [RelayCommand]
        public void ExitApplication()
        {
            Application.Current.Shutdown();
        }
    }
}
