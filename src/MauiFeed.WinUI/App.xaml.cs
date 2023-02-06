// <copyright file="App.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Reflection;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Services;
using MauiFeed.Models;
using MauiFeed.NewsService;
using MauiFeed.Services;
using MauiFeed.WinUI.Services;
using MauiFeed.WinUI.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace MauiFeed.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? window;
        private WindowService windowService;
        private ApplicationSettingsService applicationSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            string databaseField = WinUIExtensions.IsRunningAsUwp() ? System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "database.db") : Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly()!.Location!)!, "database.db");

            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            Ioc.Default.ConfigureServices(
                new ServiceCollection()
                .AddSingleton<IErrorHandlerService, WinUIErrorHandlerService>()
                .AddSingleton<IAppDispatcher>(new AppDispatcher(dispatcherQueue))
                .AddSingleton<DatabaseContext>(new DatabaseContext(databaseField))
                .AddSingleton<ITemplateService, HandlebarsTemplateService>()
                .AddSingleton<FeedService>()
                .AddSingleton<RssFeedCacheService>()
                .AddSingleton<WindowsPlatformService>()
                .AddSingleton<WindowService>()
                .AddSingleton<ApplicationSettingsService>()
                .AddSingleton<ThemeSelectorService>()
                .AddSingleton(new Progress<RssCacheFeedUpdate>())
                .AddSingleton<OpmlFeedListItemFactory>()
                .BuildServiceProvider());

            this.windowService = Ioc.Default.GetService<WindowService>()!;
            this.applicationSettingsService = Ioc.Default.GetService<ApplicationSettingsService>()!;
            this.applicationSettingsService.UpdateCulture();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            this.window = this.windowService.AddWindow<MainWindow>();
            this.window.Activate();
            this.window.SetIconFromApplicationIcon();

            Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().Activated += this.App_Activated;
        }

        private void App_Activated(object? sender, Microsoft.Windows.AppLifecycle.AppActivationArguments e)
        {
            // From https://github.com/andrewleader/RedirectActivationWinUI3Sample/blob/main/RedirectActivationWinUI3Sample/App.xaml.cs#L71-L88
            var hwnd = (Windows.Win32.Foundation.HWND)WinRT.Interop.WindowNative.GetWindowHandle(this.window);

            Windows.Win32.PInvoke.ShowWindow(hwnd, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_RESTORE);

            Windows.Win32.PInvoke.SetForegroundWindow(hwnd);
        }
    }
}
