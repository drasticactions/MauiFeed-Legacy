// <copyright file="AppDelegate.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Services;
using MauiFeed.NewsService;
using MauiFeed.Services;
using Microsoft.Extensions.DependencyInjection;
using static System.Environment;

namespace MauiFeed.MacCatalyst;

/// <summary>
/// App Delegate.
/// </summary>
[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
    private string dbpath = System.IO.Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData), "MauiFeed", "database.db");

    /// <summary>
    /// Gets or sets the UIWindow.
    /// </summary>
    public override UIWindow? Window
    {
        get;
        set;
    }

    /// <inheritdoc/>
    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        Ioc.Default.ConfigureServices(
        new ServiceCollection()
        .AddSingleton<IAppDispatcher>(new AppDispatcher())
        .AddSingleton(new DatabaseContext(this.dbpath))
        .AddSingleton<IErrorHandlerService, ErrorHandlerService>()
        .AddSingleton<ITemplateService, HandlebarsTemplateService>()
        .AddSingleton<FeedService>()
        .AddSingleton<RssFeedCacheService>()
        .BuildServiceProvider());

        // create a new window instance based on the screen size
        this.Window = new MainWindow(UIScreen.MainScreen.Bounds);

        // make the window visible
        this.Window.MakeKeyAndVisible();

        return true;
    }
}
