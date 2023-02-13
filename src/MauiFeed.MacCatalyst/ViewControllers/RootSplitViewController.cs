// <copyright file="RootSplitViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using MauiFeed.Models;

namespace MauiFeed.MacCatalyst.ViewControllers
{
    /// <summary>
    /// Root Split View Controller.
    /// </summary>
    public class RootSplitViewController : UISplitViewController, IUISplitViewControllerDelegate
    {
        private SidebarViewController sidebar;
        private TimelineTableViewController feedCollection;
        private FeedWebViewController webview;
        private Progress<RssCacheFeedUpdate> progressUpdate;

        /// <summary>
        /// Initializes a new instance of the <see cref="RootSplitViewController"/> class.
        /// </summary>
        public RootSplitViewController()
            : base(UISplitViewControllerStyle.TripleColumn)
        {
            this.progressUpdate = new Progress<RssCacheFeedUpdate>();
            this.progressUpdate.ProgressChanged += this.ProgressUpdateProgressChanged;

            this.feedCollection = new TimelineTableViewController(this);
            this.sidebar = new SidebarViewController(this);
            this.webview = new FeedWebViewController(this);

            this.SetViewController((UIViewController)this.sidebar, UISplitViewControllerColumn.Primary);
            this.SetViewController((UIViewController)this.webview, UISplitViewControllerColumn.Secondary);
            this.SetViewController((UIViewController)this.feedCollection, UISplitViewControllerColumn.Supplementary);
            this.PreferredDisplayMode = UISplitViewControllerDisplayMode.TwoBesideSecondary;

            // HOW THE F WOULD YOU GUESS THAT THIS MAKES THE SIDEBAR NOT LOOK LIKE TOTAL CRAP.
            // WHAT THE F APPLE THIS TOOK ME AN HOUR TO FIGURE OUT!!!!
            this.PrimaryBackgroundStyle = UISplitViewControllerBackgroundStyle.Sidebar;

            this.PreferredPrimaryColumnWidth = 275f;
        }

        /// <summary>
        /// Gets the sidebar view.
        /// </summary>
        public SidebarViewController Sidebar => this.sidebar;

        /// <summary>
        /// Gets the feed collection view.
        /// </summary>
        public TimelineTableViewController FeedCollection => this.feedCollection;

        /// <summary>
        /// Gets the Progress Update.
        /// </summary>
        public Progress<RssCacheFeedUpdate> ProgressUpdate => this.progressUpdate;

        private void ProgressUpdateProgressChanged(object? sender, RssCacheFeedUpdate e)
        {
            if (e.IsDone)
            {
            }
        }
    }
}