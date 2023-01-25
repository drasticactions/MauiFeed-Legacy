// <copyright file="MainToolbarDelegate.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using AppKit;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Tools;
using MauiFeed.Services;
using ObjCRuntime;
using UIKit;

namespace MauiFeed.Apple
{
    public class MainToolbarDelegate : AppKit.NSToolbarDelegate
    {
        public const string Refresh = "Refresh";
        private RootSplitViewController controller;
        private RssFeedCacheService cache;
        private UIAlertController addFeedController;
        private AddNewFeedController newFeedController;
        public const string Plus = "Plus";
        public const string MarkAllAsRead = "MarkAllAsRead";
        public const string MarkAsRead = "MarkAsRead";
        public const string HideRead = "HideRead";
        public const string Star = "Star";
        public const string NextUnread = "NextUnread";
        public const string ReaderView = "ReaderView";
        public const string Share = "Share";
        public const string OpenInBrowser = "OpenInBrowser";

        public MainToolbarDelegate(RootSplitViewController controller)
        {
            this.newFeedController = new AddNewFeedController();
            this.controller = controller;
            this.cache = (RssFeedCacheService)Ioc.Default.GetService<RssFeedCacheService>()!;
            this.addFeedController = UIAlertController.Create(Translations.Common.AddFeedButton, null, UIAlertControllerStyle.Alert);
            this.addFeedController.AddTextField((field) => { });
            var saveButtonAction = UIAlertAction.Create(Translations.Common.AddLabel, UIAlertActionStyle.Default, (alert) => {
                Uri? result = null;
                var created = Uri.TryCreate(this.addFeedController.TextFields[0].Text, UriKind.Absolute, out result);
                if (created)
                {
                    Task.Run(async () => {
                        await this.cache.RetrieveFeedAsync(result!.ToString());
                        this.controller.SidebarViewController.UpdateSidebar();
                    }).FireAndForgetSafeAsync();
                }
            });
            var cancelButtonAction = UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (alert) => { });
            this.addFeedController.AddAction(saveButtonAction);
            this.addFeedController.AddAction(cancelButtonAction);
        }

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

        public override string[] SelectableItemIdentifiers(NSToolbar toolbar)
        {
            return new string[]
             {
                HideRead,
             };
        }

        public override string[] AllowedItemIdentifiers(NSToolbar toolbar)
        {
            return new string[0];
        }

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

            toolbarItem.Activated += this.ToolbarItem_Activated;
            return toolbarItem;
        }

        private void ToolbarItem_Activated(object? sender, EventArgs e)
        {
            var toolbarItem = (NSToolbarItem)sender!;

            switch (toolbarItem.Identifier)
            {
                case Refresh:
                    Task.Run(async () =>
                    {
                        await this.cache.RefreshFeedsAsync(this.controller.ProgressUpdate);
                    });
                    break;
                case Plus:
                    this.controller.PresentViewControllerAsync(this.addFeedController, true).FireAndForgetSafeAsync();
                    break;
                case MarkAllAsRead:
                    Task.Run(async () =>
                    {
                    });
                    break;
            }
        }

        private class AddNewFeedController : UIViewController
        {
            public AddNewFeedController()
            {
                this.ModalPresentationStyle = UIModalPresentationStyle.Automatic;
                this.View!.AddSubview(new UILabel());
                this.View!.AddSubview(new UIButton());
            }
        }
    }
}