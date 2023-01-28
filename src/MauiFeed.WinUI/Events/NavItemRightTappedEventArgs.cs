// <copyright file="NavItemRightTappedEventArgs.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.WinUI.Views;

namespace MauiFeed.WinUI.Events
{
    /// <summary>
    /// Nav Items Right Tapped Event Args.
    /// </summary>
    public class NavItemRightTappedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavItemRightTappedEventArgs"/> class.
        /// </summary>
        /// <param name="item">Feed Sidebar Item.</param>
        public NavItemRightTappedEventArgs(FeedSidebarItem item)
        {
            this.SidebarItem = item;
        }

        /// <summary>
        /// Gets the feed sidebar item.
        /// </summary>
        public FeedSidebarItem SidebarItem { get; }
    }
}
