// <copyright file="ITimelineView.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using MauiFeed.Models;

namespace MauiFeed.Views
{
    /// <summary>
    /// Timeline View.
    /// </summary>
    public interface ITimelineView
    {
        /// <summary>
        /// Set and replace the feed items in a given timeline.
        /// </summary>
        /// <param name="feedItems">Feed items.</param>
        void SetFeedItems(IList<FeedItem> feedItems);

        /// <summary>
        /// Mark all feed items in the given timeline view as read.
        /// </summary>
        /// <returns>Task.</returns>
        Task MarkAllFeedItemsAsReadAsync();
    }
}