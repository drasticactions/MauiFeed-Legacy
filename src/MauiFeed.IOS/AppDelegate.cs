// <copyright file="AppDelegate.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using AngleSharp.Dom;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Services;
using MauiFeed.Apple;
using MauiFeed.Services;
using Microsoft.Extensions.DependencyInjection;
using ObjCRuntime;
using static System.Environment;

namespace MauiFeed.IOS;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
    public override UIWindow? Window
    {
        get;
        set;
    }

    private string dbpath = System.IO.Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData), "MauiFeed", "database.db");

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        this.SetupDebugDatabase();

        Ioc.Default.ConfigureServices(
        new ServiceCollection()
        .AddSingleton<IAppDispatcher>(new AppDispatcher())
        .AddSingleton<IDatabaseService>(new EFCoreDatabaseContext(dbpath))
        .AddSingleton<IErrorHandlerService, ErrorHandlerService>()
        .AddSingleton<ITemplateService, HandlebarsTemplateService>()
        .AddSingleton<IRssService, FeedReaderService>()
        .BuildServiceProvider());

        // create a new window instance based on the screen size
        this.Window = new MainWindow(UIScreen.MainScreen.Bounds);

        // make the window visible
        this.Window.MakeKeyAndVisible();

        return true;
    }

    private void SetupDebugDatabase()
    {
        if (File.Exists(dbpath))
        {
            return;
            // File.Delete(realPath);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(dbpath)!);
        var db = MauiFeed.Utilities.GetResourceFileContent("DebugFiles.database_test.db")!;
        using var feed = File.OpenWrite(dbpath);
        db.CopyTo(feed);
    }
}
