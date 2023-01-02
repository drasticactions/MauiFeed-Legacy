// <copyright file="FeedItemSelectedEventArgs.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;

namespace MauiFeed.Events
{
    public class FeedItemSelectedEventArgs : EventArgs
    {
        private readonly FeedItem feedItem;
        private readonly FeedListItem? feedListItem;

        public FeedItemSelectedEventArgs(FeedListItem? feedListItem, FeedItem item)
        {
            this.feedItem = item;
            this.feedListItem = feedListItem;
        }

        public FeedItem FeedItem => this.feedItem;

        public FeedListItem? FeedListItem => this.feedListItem;
    }
}
