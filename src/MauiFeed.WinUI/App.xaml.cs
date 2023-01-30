// <copyright file="App.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using AngleSharp.Dom;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Services;
using MauiFeed.Models;
using MauiFeed.NewsService;
using MauiFeed.Services;
using MauiFeed.WinUI.Services;
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
        private ThemeSelectorService themeSelectorService;
        private ApplicationSettingsService settingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.windowService = new WindowService();
            this.settingsService = new ApplicationSettingsService();
            this.themeSelectorService = new ThemeSelectorService(this.windowService, this.settingsService);

            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            Ioc.Default.ConfigureServices(
                new ServiceCollection()
                .AddSingleton<IErrorHandlerService, WinUIErrorHandlerService>()
                .AddSingleton<IAppDispatcher>(new AppDispatcher(dispatcherQueue))
                .AddSingleton<DatabaseContext>(new DatabaseContext(System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "database.db")))
                .AddSingleton<ITemplateService, HandlebarsTemplateService>()
                .AddSingleton<IRssService, FeedReaderService>()
                .AddSingleton<RssFeedCacheService>()
                .AddSingleton<WindowsPlatformService>()
                .AddSingleton(new Progress<RssCacheFeedUpdate>())
                .AddSingleton(this.settingsService)
                .AddSingleton(this.windowService)
                .AddSingleton(this.themeSelectorService)
                .BuildServiceProvider());
        }

        /// <summary>
        /// Gets the main app window.
        /// </summary>
        public Window? Window => this.window;

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            this.window = this.windowService.AddWindow<MainWindow>();
            this.window.Activate();

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
