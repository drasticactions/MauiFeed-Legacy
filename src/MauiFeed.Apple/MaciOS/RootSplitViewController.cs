﻿// <copyright file="RootSplitViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using MauiFeed.Services;
using MauiFeed.Views;

namespace MauiFeed.Apple
{
    public class RootSplitViewController : UISplitViewController, IUISplitViewControllerDelegate
    {
        private ISidebarView sidebar;
        private ITimelineView feedCollection;
        private IArticleView webview;
        private Progress<RssCacheFeedUpdate> progressUpdate;

        public RootSplitViewController()
            : base(UISplitViewControllerStyle.TripleColumn)
        {
            this.progressUpdate = new Progress<RssCacheFeedUpdate>();
            this.progressUpdate.ProgressChanged += ProgressUpdate_ProgressChanged;
            this.feedCollection = new TimelineCollectionViewController(this);
            this.sidebar = new SidebarViewController(this);
            this.webview = new FeedWebViewController(this);

            this.SetViewController((UIViewController)this.sidebar, UISplitViewControllerColumn.Primary);
            this.SetViewController((UIViewController)this.webview, UISplitViewControllerColumn.Secondary);
            this.SetViewController((UIViewController)this.feedCollection, UISplitViewControllerColumn.Supplementary);
            this.PreferredDisplayMode = UISplitViewControllerDisplayMode.TwoBesideSecondary;

            // HOW THE F WOULD YOU GUESS THAT THIS MAKES THE SIDEBAR NOT LOOK LIKE TOTAL CRAP.
            // WHAT THE F APPLE THIS TOOK ME AN HOUR TO FIGURE OUT!!!!
            this.PrimaryBackgroundStyle = UISplitViewControllerBackgroundStyle.Sidebar;

            this.PreferredPrimaryColumnWidth = 200f;

#if IOS
            this.PreferredSplitBehavior = UISplitViewControllerSplitBehavior.Tile;
#endif
        }

        public ISidebarView SidebarViewController => this.sidebar;

        public ITimelineView FeedViewController => this.feedCollection;

        public IArticleView FeedWebViewController => this.webview;

        public Progress<RssCacheFeedUpdate> ProgressUpdate => this.progressUpdate;

#if IOS
        [Export("splitViewController:topColumnForCollapsingToProposedTopColumn:")]
        public UISplitViewControllerColumn GetTopColumnForCollapsing(UISplitViewController splitViewController, UISplitViewControllerColumn proposedTopColumn)
        {
            return UISplitViewControllerColumn.Primary;
        }

        [Export("splitViewController:collapseSecondaryViewController:ontoPrimaryViewController:")]
        public bool CollapseSecondViewController(UISplitViewController splitViewController, UIViewController secondaryViewController, UIViewController primaryViewController)
        {
            return true;
        }
#endif

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

        private void ProgressUpdate_ProgressChanged(object? sender, RssCacheFeedUpdate e)
        {
            if (e.IsDone)
            {
                this.sidebar.UpdateSidebar();
                this.FeedViewController.UpdateFeed();
            }
        }
    }
}