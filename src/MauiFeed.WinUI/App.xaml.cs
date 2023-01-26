// <copyright file="App.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Services;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.windowService = new WindowService();
            this.themeSelectorService = new ThemeSelectorService(this.windowService);

            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            Ioc.Default.ConfigureServices(
                new ServiceCollection()
                .AddSingleton<IErrorHandlerService, WinUIErrorHandlerService>()
                .AddSingleton<DatabaseContext>(new DatabaseContext(System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "database.db")))
                .AddSingleton<ITemplateService, HandlebarsTemplateService>()
                .AddSingleton<IRssService, FeedReaderService>()
                .AddSingleton<RssFeedCacheService>()
                .AddSingleton(this.windowService)
                .AddSingleton(this.themeSelectorService)
                .BuildServiceProvider());
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            this.window = this.windowService.AddWindow<MainWindow>();
            this.window.Activate();
        }
    }
}
