// <copyright file="MainWindow.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;

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
            this.RootViewController = new UIViewController();
        }
    }
}