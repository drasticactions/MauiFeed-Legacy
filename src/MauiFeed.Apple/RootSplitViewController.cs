// <copyright file="RootSplitViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

#if !TVOS

using System;

namespace MauiFeed.Apple
{
    public class RootSplitViewController : UISplitViewController, IUISplitViewControllerDelegate
    {
        private SidebarViewController sidebar;
        private FeedTableViewController feedCollection;
        private FeedWebViewController webview;

        public RootSplitViewController()
            : base(UISplitViewControllerStyle.TripleColumn)
        {
            this.feedCollection = new FeedTableViewController(this);
            this.sidebar = new SidebarViewController(this);
            this.webview = new FeedWebViewController(this);

            this.SetViewController(this.sidebar, UISplitViewControllerColumn.Primary);
            this.SetViewController(this.webview, UISplitViewControllerColumn.Secondary);
            this.SetViewController(this.feedCollection, UISplitViewControllerColumn.Supplementary);
            this.PreferredDisplayMode = UISplitViewControllerDisplayMode.TwoBesideSecondary;

            // HOW THE F WOULD YOU GUESS THAT THIS MAKES THE SIDEBAR NOT LOOK LIKE TOTAL CRAP.
            // WHAT THE F APPLE THIS TOOK ME AN HOUR TO FIGURE OUT!!!!
            this.PrimaryBackgroundStyle = UISplitViewControllerBackgroundStyle.Sidebar;

            this.PreferredPrimaryColumnWidth = 200f;

#if IOS
            this.PreferredSplitBehavior = UISplitViewControllerSplitBehavior.Tile;
#endif
        }

        public SidebarViewController SidebarViewController => this.sidebar;

        public FeedTableViewController FeedTableViewController => this.feedCollection;

        public FeedWebViewController FeedWebViewController => this.webview;

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            this.Delegate = this;
        }

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
    }
}

#endif