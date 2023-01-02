// <copyright file="FeedListItemSelectedEventArgs.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;

namespace MauiFeed.Events
{
    public class FeedListItemSelectedEventArgs : EventArgs
    {
        private readonly FeedListItem feedItem;

        public FeedListItemSelectedEventArgs(FeedListItem item)
        {
            this.feedItem = item;
        }

        public FeedListItem FeedListItem => this.feedItem;
    }
}
