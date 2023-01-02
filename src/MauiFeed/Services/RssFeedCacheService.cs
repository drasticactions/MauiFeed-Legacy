// <copyright file="RssFeedCacheService.cs" company="Drastic Actions">
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

        public async Task<FeedListItem> RetrieveFeedAsync(string feedUri)
        {
            // First, get the feed no matter what.
            (var feed, var feedListItems) = await this.rssService.ReadFeedAsync(feedUri);

            // Next, upsert the feed list item into the database. This will get us the Id...
            feed = await this.databaseContext.AddOrUpdateFeedListItem(feed!);

            foreach (var item in feedListItems!)
            {
                // ... that we will then set for the individual items to link back to this one.
                item.FeedListItemId = feed.Id;
                await this.databaseContext.AddOrUpdateFeedItem(item);
            }

            feed.Items = feedListItems;

            return feed;
        }
    }
}
