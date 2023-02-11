// <copyright file="MainToolbarDelegate.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using MauiFeed.MacCatalyst.ViewControllers;

namespace MauiFeed.MacCatalyst.Toolbar
{
    /// <summary>
    /// Main Toolbar Delegate.
    /// </summary>
    public class MainToolbarDelegate : AppKit.NSToolbarDelegate
    {
        private RootSplitViewController controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainToolbarDelegate"/> class.
        /// </summary>
        /// <param name="controller">Root View Controller.</param>
        public MainToolbarDelegate(RootSplitViewController controller)
        {
            this.controller = controller;
        }
    }
}