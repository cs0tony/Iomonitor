using Iomonitor.Models;
using Iomonitor.Services;
using Iomonitor.ViewModels.Pages;
using Iomonitor.ViewModels.Windows;
using Iomonitor.Views.Pages;
using Iomonitor.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Wpf.Ui;
using H.NotifyIcon;

namespace Iomonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)); })
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<ApplicationHostService>();

                // Page resolver service
                services.AddSingleton<IPageService, PageService>();

                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IContentDialogService, ContentDialogService>();

                // Main window with navigation
                services.AddSingleton<INavigationWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();

                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<HotkeysPage>();
                services.AddSingleton<HotkeysViewModel>();
                services.AddSingleton<AboutPage>();
                services.AddSingleton<AboutViewModel>();
            }).Build();

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        public static readonly Configuration Cfg = GetCfg();

        public static Configuration GetCfg()
        {
            Configuration cfg = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (cfg.GetSection("userSettings") == null)
            {
                cfg.Sections.Add("userSettings", new UserSettings());
                cfg.Save();
            }
            return cfg;
        }

        private static Mutex mutex;

        private TaskbarIcon? notifyIcon;

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            const string mutexName = "Iomonitor-Tony.Inc"; // 这里可以使用你应用程序的唯一名称
            bool isOwned;

            mutex = new Mutex(true, mutexName, out isOwned);

            if (!isOwned)
            {
                // 另一个实例已经在运行，退出当前实例
                MessageBox.Show("该应用程序已经在运行。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Shutdown();
                return;
            }
            _host.Start();
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            notifyIcon.ForceCreate();
            notifyIcon.ContextMenu.Loaded += TrayContextMenuLoaded;
        }

        private void TrayContextMenuLoaded(object sender, RoutedEventArgs e)
        {
            UserSettings.Instance.ChangeLanguage(UserSettings.Instance.SelectedLanguage);
        }

        private void ExitApplication(object? sender, EventArgs e)
        {
            // 关闭应用程序
            this.Shutdown();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            // 释放互斥锁
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex = null;
            }
            CursorLock.UnlockCursor();
            notifyIcon?.Dispose(); //the icon would clean up automatically, but this is cleaner
            await _host.StopAsync();
            
            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"发生错误: {e.Exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
    }
}
