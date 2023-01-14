// <copyright file="RssFeedCacheService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using CodeHollow.FeedReader;
using MauiFeed.Events;
using MauiFeed.Models;
using System.Reflection.Metadata.Ecma335;

namespace MauiFeed.Services
{
    public class RssFeedCacheService
    {
        private IRssService rssService;
        private DatabaseContext databaseContext;

        public RssFeedCacheService(IRssService rssService, DatabaseContext databaseContext)
        {
            this.rssService = rssService;
            this.databaseContext = databaseContext;
        }

        public Task<FeedListItem> RetrieveFeedAsync(Uri feedUri)
            => this.RetrieveFeedAsync(feedUri.ToString());

        public async Task RefreshFeedsAsync(IProgress<RssCacheFeedUpdate>? progress = default)
        {
            var feeds = await this.databaseContext.GetAllFeedListAsync();
            await this.RefreshFeedsAsync(feeds, progress);
        }

        public async Task RefreshFeedAsync(FeedListItem item, IProgress<RssCacheFeedUpdate>? progress = default)
        {
            await this.RefreshFeedsAsync(new List<FeedListItem>() { item }, progress);
        }

        public async Task RefreshFeedsAsync(List<FeedListItem> feeds, IProgress<RssCacheFeedUpdate>? progress = default)
        {
            for (int i = 0; i < feeds.Count; i++)
            {
                FeedListItem? feed = feeds[i];
                var updatedItem = await this.RetrieveFeedAsync(feed.Uri!.ToString());
                progress?.Report(new RssCacheFeedUpdate(i + 1, feeds.Count, updatedItem));
            }
        }

        public async Task<FeedListItem> RetrieveFeedAsync(string feedUri)
        {
            // First, get the feed no matter what.
            (var feed, var feedListItems) = await this.rssService.ReadFeedAsync(feedUri);

            var oldFeed = await this.databaseContext.GetFeedListItem(feed!.Uri);

            if (oldFeed is null)
            {
                oldFeed = await this.databaseContext.AddFeedListItem(feed!);
            }

            foreach (var item in feedListItems!)
            {
                // ... that we will then set for the individual items to link back to this one.
                var oldItem = await this.databaseContext.GetFeedItemViaRssId(item.RssId);
                if (oldItem is not null)
                {
                    continue;
                }

                item.FeedListItemId = oldFeed.Id;
                item.Feed = oldFeed;
                await this.databaseContext.AddFeedItem(item);
            }

            feed.Items = feedListItems;

            return feed;
        }
    }

    public class RssCacheFeedUpdate
    {
        public RssCacheFeedUpdate(int feedsCompleted, int totalFeeds, FeedListItem lastUpdated)
        {
            this.LastUpdated = lastUpdated;
            this.TotalFeeds = totalFeeds;
            this.FeedsCompleted = feedsCompleted;
        }

        public FeedListItem LastUpdated { get; }

        public int TotalFeeds { get; }

        public int FeedsCompleted { get; }

        public bool IsDone => this.FeedsCompleted >= this.TotalFeeds;
    }
}
