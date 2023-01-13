// <copyright file="MainWindow.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;

#if MACCATALYST
using AppKit;
#endif

namespace MauiFeed.Apple
{
    public class MainWindow : UIWindow
    {
        public MainWindow(CGRect frame)
           : base(frame)
        {
            this.RootViewController = new RootSplitViewController();
#if MACCATALYST
            var windowScene = this.WindowScene;

            if (windowScene is not null)
            {
                windowScene.Titlebar!.TitleVisibility = UITitlebarTitleVisibility.Visible;

                var toolbar = new NSToolbar();
                toolbar.Delegate = new MainToolbarDelegate((RootSplitViewController)this.RootViewController);
                toolbar.DisplayMode = NSToolbarDisplayMode.Icon;

                windowScene.Title = MauiFeed.Translations.Common.AppTitle;
                windowScene.Titlebar.Toolbar = toolbar;
                windowScene.Titlebar.ToolbarStyle = UITitlebarToolbarStyle.Automatic;
                windowScene.Titlebar.Toolbar.Visible = true;
            }
#endif
        }
    }
}