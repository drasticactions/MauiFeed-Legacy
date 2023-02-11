// <copyright file="MainToolbarDelegate.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using AppKit;
using MauiFeed.MacCatalyst.ViewControllers;

namespace MauiFeed.MacCatalyst.Toolbar
{
    /// <summary>
    /// Main Toolbar Delegate.
    /// </summary>
    public class MainToolbarDelegate : AppKit.NSToolbarDelegate
    {
        private const string Refresh = "Refresh";

        private RootSplitViewController controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainToolbarDelegate"/> class.
        /// </summary>
        /// <param name="controller">Root View Controller.</param>
        public MainToolbarDelegate(RootSplitViewController controller)
        {
            this.controller = controller;
        }

        /// <inheritdoc/>
        public override string[] DefaultItemIdentifiers(NSToolbar toolbar)
        {
            // https://github.com/xamarin/xamarin-macios/issues/12871
            // I only figured this out by going into a Catalyst Swift app,
            // and checking the raw value for NSToolbarItem.Identifier.primarySidebarTrackingSeparatorItemIdentifier
            // This value is not bound yet in dotnet maccatalyst.
            return new string[]
            {
                NSToolbar.NSToolbarFlexibleSpaceItemIdentifier,
                Refresh,
            };
        }

        /// <inheritdoc/>
        public override NSToolbarItem? WillInsertItem(NSToolbar toolbar, string itemIdentifier, bool willBeInserted)
        {
            NSToolbarItem toolbarItem = new NSToolbarItem(itemIdentifier);

            if (itemIdentifier == Refresh)
            {
                toolbarItem.UIImage = UIImage.GetSystemImage("arrow.clockwise");
            }

            return toolbarItem;
        }
    }
}