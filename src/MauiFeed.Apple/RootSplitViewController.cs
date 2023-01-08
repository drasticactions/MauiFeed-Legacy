// <copyright file="RootSplitViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

#if !TVOS

using System;

namespace MauiFeed.Apple
{
    public class RootSplitViewController : UISplitViewController
    {
        private SidebarViewController sidebar;
        private FeedTableViewController feedCollection;

        public RootSplitViewController()
            : base(UISplitViewControllerStyle.TripleColumn)
        {
            this.feedCollection = new FeedTableViewController();
            this.sidebar = new SidebarViewController(this.feedCollection);

            this.SetViewController(this.sidebar, UISplitViewControllerColumn.Primary);
            this.SetViewController(new UIViewController(), UISplitViewControllerColumn.Secondary);
            this.SetViewController(this.feedCollection, UISplitViewControllerColumn.Supplementary);
            this.PreferredDisplayMode = UISplitViewControllerDisplayMode.TwoBesideSecondary;

            // HOW THE F WOULD YOU GUESS THAT THIS MAKES THE SIDEBAR NOT LOOK LIKE TOTAL CRAP.
            // WHAT THE F APPLE THIS TOOK ME AN HOUR TO FIGURE OUT!!!!
            this.PrimaryBackgroundStyle = UISplitViewControllerBackgroundStyle.Sidebar;

            this.PreferredPrimaryColumnWidth = 200f;
        }
    }
}

#endif