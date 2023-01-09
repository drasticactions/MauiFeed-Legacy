﻿// <copyright file="RssFeedCacheService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using CodeHollow.FeedReader;
using MauiFeed.Events;
using MauiFeed.Models;

namespace MauiFeed.Services
{
    public class RssFeedCacheService
    {
        private IRssService rssService;
        private IDatabaseService databaseContext;

        public RssFeedCacheService(IRssService rssService, IDatabaseService databaseContext)
        {
            this.rssService = rssService;
            this.databaseContext = databaseContext;
        }

        public Task<FeedListItem> RetrieveFeedAsync(Uri feedUri)
            => this.RetrieveFeedAsync(feedUri.ToString());

        public async Task RefreshFeedsAsync()
        {
            var feeds = await this.databaseContext.GetAllFeedListAsync();
            foreach (var feed in feeds)
            {
                await this.RetrieveFeedAsync(feed.Uri!.ToString());
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
                await this.databaseContext.AddFeedItem(item);
            }

            feed.Items = feedListItems;

            return feed;
        }
    }
}
