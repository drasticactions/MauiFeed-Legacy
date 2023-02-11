// <copyright file="RootSplitViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;

namespace MauiFeed.MacCatalyst.ViewControllers
{
    /// <summary>
    /// Root Split View Controller.
    /// </summary>
    public class RootSplitViewController : UISplitViewController
    {
        private SidebarViewController sidebar;
        private TimelineCollectionViewController feedCollection;
        private FeedWebViewController webview;

        /// <summary>
        /// Initializes a new instance of the <see cref="RootSplitViewController"/> class.
        /// </summary>
        public RootSplitViewController()
            : base(UISplitViewControllerStyle.TripleColumn)
        {
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
        }
    }
}