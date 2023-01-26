// <copyright file="RssFeedCacheService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;
using Microsoft.EntityFrameworkCore;

namespace MauiFeed.Services
{
    /// <summary>
    /// Rss Feed Cache Service.
    /// </summary>
    public class RssFeedCacheService
    {
        private IRssService rssService;
        private DatabaseContext databaseContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RssFeedCacheService"/> class.
        /// </summary>
        /// <param name="rssService">Rss Service.</param>
        /// <param name="databaseContext">Database Context.</param>
        public RssFeedCacheService(IRssService rssService, DatabaseContext databaseContext)
        {
            this.rssService = rssService;
            this.databaseContext = databaseContext;
        }

        /// <summary>
        /// Retrieve Feed via URI.
        /// </summary>
        /// <param name="feedUri">Feed Uri.</param>
        /// <returns>FeedListItem.</returns>
        public Task<FeedListItem> RetrieveFeedAsync(Uri feedUri)
            => this.RetrieveFeedAsync(feedUri.ToString());

        /// <summary>
        /// Refresh Feeds Async.
        /// </summary>
        /// <param name="progress">Optional Progress Marker.</param>
        /// <returns>Task.</returns>
        public async Task RefreshFeedsAsync(IProgress<RssCacheFeedUpdate>? progress = default)
        {
            var feeds = await this.databaseContext.FeedListItems!.ToListAsync();
            await this.RefreshFeedsAsync(feeds, progress);
        }

        /// <summary>
        /// Refresh Feed.
        /// </summary>
        /// <param name="item">FeedListItem to update.</param>
        /// <param name="progress">Optional Progress Marker.</param>
        /// <returns>Task.</returns>
        public async Task RefreshFeedAsync(FeedListItem item, IProgress<RssCacheFeedUpdate>? progress = default)
        {
            await this.RefreshFeedsAsync(new List<FeedListItem>() { item }, progress);
        }

        /// <summary>
        /// Refresh Feeds Async.
        /// </summary>
        /// <param name="feeds">Feeds.</param>
        /// <param name="progress">Optional Progress Marker.</param>
        /// <returns>Task.</returns>
        public async Task RefreshFeedsAsync(List<FeedListItem> feeds, IProgress<RssCacheFeedUpdate>? progress = default)
        {
            for (int i = 0; i < feeds.Count; i++)
            {
                FeedListItem? feed = feeds[i];
                var updatedItem = await this.RetrieveFeedAsync(feed.Uri!.ToString());
                progress?.Report(new RssCacheFeedUpdate(i + 1, feeds.Count, updatedItem));
            }
        }

        /// <summary>
        /// Retrieve and update feed async.
        /// </summary>
        /// <param name="feedUri">FeedUri.</param>
        /// <returns>Task of FeedListItem.</returns>
        public async Task<FeedListItem> RetrieveFeedAsync(string feedUri)
        {
            // First, get the feed no matter what.
            (var feed, var feedListItems) = await this.rssService.ReadFeedAsync(feedUri);

            var oldFeed = await this.databaseContext.FeedListItems!.FirstOrDefaultAsync(n => feed!.Uri == n.Uri);

            if (oldFeed is null)
            {
                await this.databaseContext.FeedListItems!.AddAsync(feed!);
            }

            foreach (var item in feedListItems!)
            {
                // ... that we will then set for the individual items to link back to this one.
                var oldItem = await this.databaseContext.FeedItems!.FirstOrDefaultAsync(n => n.RssId == item.RssId);
                if (oldItem is not null)
                {
                    continue;
                }

                item.FeedListItemId = oldFeed!.Id;
                item.Feed = oldFeed;
                await this.databaseContext.FeedItems!.AddAsync(item);
            }

            feed!.Items = feedListItems;

            return feed;
        }
    }
}
