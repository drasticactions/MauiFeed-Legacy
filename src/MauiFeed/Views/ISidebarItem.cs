// <copyright file="ISidebarItem.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;

namespace MauiFeed.Views
{
    /// <summary>
    /// Sidebar Item.
    /// </summary>
    public interface ISidebarItem
    {
        /// <summary>
        /// Gets the selected feed list item. Optional.
        /// </summary>
        FeedListItem? FeedListItem { get; }

        /// <summary>
        /// Gets the selected feed folder item. Optional.
        /// </summary>
        FeedFolder? FeedFolder { get; }

        /// <summary>
        /// Gets the feed items.
        /// </summary>
        IList<FeedItem> Items { get; }

        /// <summary>
        /// Gets the title of the sidebar item.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the query to generate the Items.
        /// </summary>
        IQueryable<FeedItem>? Query { get; }

        /// <summary>
        /// Gets the sidebar item type.
        /// </summary>
        SidebarItemType ItemType { get; }

        /// <summary>
        /// Update the inner sidebar item.
        /// </summary>
        void Update();
    }
}