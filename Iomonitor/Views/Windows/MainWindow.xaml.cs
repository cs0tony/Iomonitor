using Iomonitor.Models;
using Iomonitor.ViewModels.Windows;
using System.ComponentModel;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Iomonitor.Views.Windows
{
    public partial class MainWindow : INavigationWindow
    {
        public MainWindowViewModel ViewModel { get; }

        public MainWindow(
            MainWindowViewModel viewModel,
            IPageService pageService,
            INavigationService navigationService,
            IContentDialogService contentDialogService
        )
        {
            ViewModel = viewModel;
            DataContext = this;

            // SystemThemeWatcher.Watch(this);

            InitializeComponent();
            SetPageService(pageService);

            navigationService.SetNavigationControl(RootNavigation);
            contentDialogService.SetDialogHost(RootContentDialog);
        }

        #region INavigationWindow methods

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService) => RootNavigation.SetPageService(pageService);

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UserSettings.Instance.ChangeTheme(UserSettings.Instance.SelectedThemeIndex);
            UserSettings.Instance.ChangeLanguage(UserSettings.Instance.SelectedLanguage);
            this.Navigate(typeof(Views.Pages.SettingsPage));
        }

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }

        private void ExitApplication(object sender,  EventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // 取消关闭请求
            e.Cancel = true;
            this.Hide();
            base.OnClosing(e);
        }

        INavigationView INavigationWindow.GetNavigation()
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}
