// <copyright file="FeedFolderDropEventArgs.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Views;
using MauiFeed.WinUI.Views;

namespace MauiFeed.WinUI.Events
{
    /// <summary>
    /// Feed Folder Drop Event Args.
    /// </summary>
    public class FeedFolderDropEventArgs : EventArgs
    {
        private FeedSidebarItem folder;
        private Guid feedListItemId;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedFolderDropEventArgs"/> class.
        /// </summary>
        /// <param name="sidebarItemId">Feed List Item Id.</param>
        /// <param name="folder">Sidebar Item.</param>
        public FeedFolderDropEventArgs(Guid sidebarItemId, FeedSidebarItem folder)
        {
            this.folder = folder;
            this.feedListItemId = sidebarItemId;
        }

        /// <summary>
        /// Gets the feed sidebar item id.
        /// </summary>
        public Guid FeedSidebarItemId => this.feedListItemId;

        /// <summary>
        /// Gets the sidebar item.
        /// </summary>
        public FeedSidebarItem Folder => this.folder;
    }
}
