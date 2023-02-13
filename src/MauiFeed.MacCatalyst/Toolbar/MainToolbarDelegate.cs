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
        private const string Plus = "Plus";
        private const string MarkAllAsRead = "MarkAllAsRead";
        private const string MarkAsRead = "MarkAsRead";
        private const string HideRead = "HideRead";
        private const string Star = "Star";
        private const string NextUnread = "NextUnread";
        private const string ReaderView = "ReaderView";
        private const string Share = "Share";
        private const string OpenInBrowser = "OpenInBrowser";

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
        public override string[] SelectableItemIdentifiers(NSToolbar toolbar)
        {
            return new string[] { HideRead, };
        }

        /// <inheritdoc/>
        public override string[] AllowedItemIdentifiers(NSToolbar toolbar)
        {
            return new string[0];
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
                Plus,
                "NSToolbarPrimarySidebarTrackingSeparatorItem",
                NSToolbar.NSToolbarFlexibleSpaceItemIdentifier,
                MarkAllAsRead,
                HideRead,
                "NSToolbarSupplementarySidebarTrackingSeparatorItem",
                MarkAsRead,
                Star,
                NextUnread,
                ReaderView,
                Share,
                OpenInBrowser,
            };
        }

        /// <inheritdoc/>
        public override NSToolbarItem? WillInsertItem(NSToolbar toolbar, string itemIdentifier, bool willBeInserted)
        {
            NSToolbarItem toolbarItem = new NSToolbarItem(itemIdentifier);

            if (itemIdentifier == Plus)
            {
                toolbarItem.UIImage = UIImage.GetSystemImage("plus");
            }
            else if (itemIdentifier == Refresh)
            {
                toolbarItem.UIImage = UIImage.GetSystemImage("arrow.clockwise");
            }
            else if (itemIdentifier == MarkAllAsRead)
            {
                toolbarItem.UIImage = UIImage.GetSystemImage("arrow.up.arrow.down.circle");
            }
            else if (itemIdentifier == HideRead)
            {
                toolbarItem.UIImage = UIImage.GetSystemImage("circle");
            }
            else if (itemIdentifier == MarkAsRead)
            {
                toolbarItem.UIImage = UIImage.GetSystemImage("book.circle");
            }
            else if (itemIdentifier == Star)
            {
                toolbarItem.UIImage = UIImage.GetSystemImage("star");
            }
            else if (itemIdentifier == NextUnread)
            {
                toolbarItem.UIImage = UIImage.GetSystemImage("arrowtriangle.down.circle");
            }
            else if (itemIdentifier == OpenInBrowser)
            {
                toolbarItem.UIImage = UIImage.GetSystemImage("safari");
            }
            else if (itemIdentifier == ReaderView)
            {
                toolbarItem.UIImage = UIImage.GetSystemImage("note.text");
            }
            else if (itemIdentifier == Share)
            {
                toolbarItem.UIImage = UIImage.GetSystemImage("square.and.arrow.up");
            }

            return toolbarItem;
        }
    }
}