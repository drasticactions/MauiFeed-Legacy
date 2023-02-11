// <copyright file="MainWindow.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using AppKit;
using MauiFeed.MacCatalyst.Toolbar;
using MauiFeed.MacCatalyst.ViewControllers;

namespace MauiFeed.MacCatalyst
{
    /// <summary>
    /// Main Window.
    /// </summary>
    public class MainWindow : UIWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// <param name="frame">Frame.</param>
        public MainWindow(CGRect frame)
            : base(frame)
        {
            this.RootViewController = new RootSplitViewController();

            var windowScene = this.WindowScene;

            if (windowScene is not null)
            {
#pragma warning disable CA1416 // プラットフォームの互換性を検証
                windowScene.Titlebar!.TitleVisibility = UITitlebarTitleVisibility.Visible;

                var toolbar = new NSToolbar();
                toolbar.Delegate = new MainToolbarDelegate((RootSplitViewController)this.RootViewController);
                toolbar.DisplayMode = NSToolbarDisplayMode.Icon;

                windowScene.Title = MauiFeed.Translations.Common.AppTitle;
                windowScene.Titlebar.Toolbar = toolbar;
                windowScene.Titlebar.ToolbarStyle = UITitlebarToolbarStyle.Automatic;
                windowScene.Titlebar.Toolbar.Visible = true;
#pragma warning restore CA1416 // プラットフォームの互換性を検証
            }
        }
    }
}