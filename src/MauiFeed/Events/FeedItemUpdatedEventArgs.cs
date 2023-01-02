// <copyright file="FeedItemUpdatedEventArgs.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;

namespace MauiFeed.Events
{
    public class FeedItemUpdatedEventArgs : EventArgs
    {
        private readonly FeedItem feedItem;

        public FeedItemUpdatedEventArgs(FeedItem item)
        {
            this.feedItem = item;
        }

        public FeedItem FeedItem => this.feedItem;
    }
}
