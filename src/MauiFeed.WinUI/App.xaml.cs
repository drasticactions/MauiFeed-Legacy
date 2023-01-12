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

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.SetupDebugDatabase();
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            Ioc.Default.ConfigureServices(
                new ServiceCollection()
                .AddSingleton<IAppDispatcher>(new AppDispatcher(dispatcherQueue))
                .AddSingleton<DatabaseContext>(new DatabaseContext(System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "database.db")))
                .AddSingleton<IErrorHandlerService, ErrorHandlerService>()
                .AddSingleton<ITemplateService, HandlebarsTemplateService>()
                .AddSingleton<IRssService, FeedReaderService>()
                .BuildServiceProvider());
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            this.window = new MainWindow();
            this.window.Activate();
        }

        private void SetupDebugDatabase()
        {
            var realPath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "database.db");
            if (File.Exists(realPath))
            {
                return;

                // File.Delete(realPath);
            }

            var db = MauiFeed.Utilities.GetResourceFileContent("DebugFiles.database_test.db")!;
            using var feed = File.OpenWrite(realPath);
            db.CopyTo(feed);
        }
    }
}
