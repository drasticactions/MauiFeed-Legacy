// <copyright file="IDatabaseService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;

namespace MauiFeed.Services
{
    public interface IDatabaseService
    {
        public int GetUnreadCountForFeedListItem(FeedListItem item);

        public Task<FeedListItem> AddFeedListItem(FeedListItem feedListItem);

        public Task<FeedItem> AddFeedItem(FeedItem item);

        public Task<FeedListItem> UpdateFeedListItem(FeedListItem feedListItem);

        public Task<FeedItem> UpdateFeedItem(FeedItem item);

        public Task<List<FeedListItem>> GetAllFeedListAsync();

        public Task<int> AddFeedItems(IList<FeedItem> feedItems);

        public Task<FeedItem?> GetFeedItemViaRssId(string? rssId);

        public Task<FeedListItem?> GetFeedListItem(Uri? rssId);
    }
}
