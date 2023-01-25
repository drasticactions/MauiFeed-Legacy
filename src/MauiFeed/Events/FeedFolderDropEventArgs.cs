// <copyright file="FeedFolderDropEventArgs.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Views;

namespace MauiFeed.Events
{
    public class FeedFolderDropEventArgs : EventArgs
    {
        private int feedListItemId;

        private ISidebarItem folder;

        public FeedFolderDropEventArgs(ISidebarItem folder, int feedListItemId)
        {
            this.feedListItemId = feedListItemId;
            this.folder = folder;
        }

        public int FeedListItemId => this.feedListItemId;

        public ISidebarItem Folder => this.folder;
    }
}
