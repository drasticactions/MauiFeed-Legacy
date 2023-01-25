// <copyright file="ISidebarItem.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;

namespace MauiFeed.Views
{
    public interface ISidebarItem
    {
        FeedListItem? FeedListItem { get; }

        /// <summary>
        /// Update the inner sidebar item.
        /// </summary>
        void Update();

        IList<FeedItem> Items { get; }

        string Title { get; }

        SidebarItemType ItemType { get; }
    }
}
