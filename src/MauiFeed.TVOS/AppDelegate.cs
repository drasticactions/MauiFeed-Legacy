// <copyright file="AppDelegate.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Services;
using MauiFeed.Apple;
using MauiFeed.NewsService;
using MauiFeed.Services;
using Microsoft.Extensions.DependencyInjection;
using static System.Environment;

namespace MauiFeed.TVOS;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
    private string dbpath = System.IO.Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData), "MauiFeed", "database.db");

    public override UIWindow? Window
    {
        get;
        set;
    }

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        this.SetupDebugDatabase();

        Ioc.Default.ConfigureServices(
        new ServiceCollection()
        .AddSingleton<IAppDispatcher>(new MauiFeed.Apple.AppDispatcher())
        .AddSingleton(new DatabaseContext(this.dbpath))
        .AddSingleton<IErrorHandlerService, ErrorHandlerService>()
        .AddSingleton<ITemplateService, HandlebarsTemplateService>()
        .AddSingleton<IRssService, FeedReaderService>()
        .AddSingleton<RssFeedCacheService>()
        .BuildServiceProvider());

        this.Window = new MainWindow(UIScreen.MainScreen.Bounds);

        this.Window.MakeKeyAndVisible();

        return true;
    }

    private void SetupDebugDatabase()
    {
        if (File.Exists(this.dbpath))
        {
            return;

            // File.Delete(dbpath);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(this.dbpath)!);
        var db = MauiFeed.Utilities.GetResourceFileContent("DebugFiles.database_test.db")!;
        using var feed = File.OpenWrite(this.dbpath);
        db.CopyTo(feed);
    }
}
