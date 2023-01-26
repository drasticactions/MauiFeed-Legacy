// <copyright file="IRssService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;

namespace MauiFeed.Services
{
    /// <summary>
    /// Rss Service.
    /// </summary>
    public interface IRssService
    {
        /// <summary>
        /// Read feed from a given Uri.
        /// </summary>
        /// <param name="feedUri">Feed Uri.</param>
        /// <param name="type">The type of feed to be read, <see cref="FeedListItemType"/>. Defaults to <see cref="FeedListItemType.Local"/>.</param>
        /// <param name="token">Optional CancellationToken.</param>
        /// <returns>One feed list item and the list of feed items.</returns>
        Task<(FeedListItem? FeedList, IList<FeedItem>? FeedItemList)> ReadFeedAsync(string feedUri, FeedListItemType type = FeedListItemType.Local, CancellationToken? token = default);
    }
}
