// <copyright file="ISidebarItem.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;

namespace MauiFeed.Views
{
    public interface ISidebarItem
    {
        /// <summary>
        /// Update the inner sidebar item.
        /// </summary>
        void Update();

        FeedListItem? FeedListItem { get; }

        IList<FeedItem> Items { get; }

        string Title { get; }
    }
}
