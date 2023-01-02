// <copyright file="IDatabaseService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;

namespace MauiFeed.Services
{
    public interface IDatabaseService
    {
        public int GetUnreadCountForFeedListItem(FeedListItem item);

        public Task<FeedListItem> AddOrUpdateFeedListItem(FeedListItem feedListItem);

        public Task<FeedItem> AddOrUpdateFeedItem(FeedItem item);

        public Task<FeedListItem> UpdateFeedListItem(FeedListItem feedListItem);

        public Task<FeedItem> UpdateFeedItem(FeedItem item);
    }
}
