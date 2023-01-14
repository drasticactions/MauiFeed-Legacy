// <copyright file="DatabaseEvents.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;

namespace MauiFeed.Events
{
    public class FeedItemsUpdatedEventArgs : EventArgs
    {
        private readonly IEnumerable<FeedItem> feedItem;
        private readonly IEnumerable<FeedListItem> feeds;

        public FeedItemsUpdatedEventArgs(IEnumerable<FeedItem> item)
        {
            this.feedItem = item;
            this.feeds = this.feedItem.Where(n => n.Feed is not null).Select(n => n.Feed).Distinct()!;
        }

        public FeedItemsUpdatedEventArgs(FeedItem item)
        {
            this.feedItem = new List<FeedItem>() { item };
            this.feeds = this.feedItem.Where(n => n.Feed is not null).Select(n => n.Feed).Distinct()!;
        }

        public IEnumerable<FeedItem> FeedItems => this.feedItem;

        public IEnumerable<FeedListItem> Feeds => this.feeds;
    }

    public class FeedItemsAddedEventArgs : EventArgs
    {
        private readonly IEnumerable<FeedItem> feedItem;
        private readonly IEnumerable<FeedListItem> feeds;

        public FeedItemsAddedEventArgs(IEnumerable<FeedItem> item)
        {
            this.feedItem = item;
            this.feeds = this.feedItem.Where(n => n.Feed is not null).Select(n => n.Feed).Distinct()!;
        }

        public FeedItemsAddedEventArgs(FeedItem item)
        {
            this.feedItem = new List<FeedItem>() { item };
            this.feeds = this.feedItem.Where(n => n.Feed is not null).Select(n => n.Feed).Distinct()!;
        }

        public IEnumerable<FeedItem> FeedItems => this.feedItem;

        public IEnumerable<FeedListItem> Feeds => this.feeds;
    }

    public class FeedItemsRemovedEventArgs : EventArgs
    {
        private readonly IEnumerable<FeedItem> feedItem;
        private readonly IEnumerable<FeedListItem> feeds;

        public FeedItemsRemovedEventArgs(IEnumerable<FeedItem> item)
        {
            this.feedItem = item;
            this.feeds = this.feedItem.Where(n => n.Feed is not null).Select(n => n.Feed).Distinct()!;
        }

        public FeedItemsRemovedEventArgs(FeedItem item)
        {
            this.feedItem = new List<FeedItem>() { item };
            this.feeds = this.feedItem.Where(n => n.Feed is not null).Select(n => n.Feed).Distinct()!;
        }

        public IEnumerable<FeedItem> FeedItems => this.feedItem;

        public IEnumerable<FeedListItem> Feeds => this.feeds;
    }

    public class FeedListItemsUpdatedEventArgs : EventArgs
    {
        private readonly IEnumerable<FeedListItem> feeds;

        public FeedListItemsUpdatedEventArgs(IEnumerable<FeedListItem> item)
        {
            this.feeds = item;
        }

        public FeedListItemsUpdatedEventArgs(FeedListItem item)
        {
            this.feeds = new List<FeedListItem>() { item };
        }

        public IEnumerable<FeedListItem> Feeds => this.feeds;
    }

    public class FeedListItemsAddedEventArgs : EventArgs
    {
        private readonly IEnumerable<FeedListItem> feeds;

        public FeedListItemsAddedEventArgs(IEnumerable<FeedListItem> item)
        {
            this.feeds = item;
        }

        public FeedListItemsAddedEventArgs(FeedListItem item)
        {
            this.feeds = new List<FeedListItem>() { item };
        }

        public IEnumerable<FeedListItem> Feeds => this.feeds;
    }

    public class FeedListItemsRemovedEventArgs : EventArgs
    {
        private readonly IEnumerable<FeedListItem> feeds;

        public FeedListItemsRemovedEventArgs(IEnumerable<FeedListItem> item)
        {
            this.feeds = item;
        }

        public FeedListItemsRemovedEventArgs(FeedListItem item)
        {
            this.feeds = new List<FeedListItem>() { item };
        }

        public IEnumerable<FeedListItem> Feeds => this.feeds;
    }

    public class FeedFoldersItemsAddedEventArgs : EventArgs
    {
        private readonly IEnumerable<FeedFolder> feeds;

        public FeedFoldersItemsAddedEventArgs(IEnumerable<FeedFolder> item)
        {
            this.feeds = item;
        }

        public FeedFoldersItemsAddedEventArgs(FeedFolder item)
        {
            this.feeds = new List<FeedFolder>() { item };
        }

        public IEnumerable<FeedFolder> Feeds => this.feeds;
    }

    public class FeedFoldersRemovedEventArgs : EventArgs
    {
        private readonly IEnumerable<FeedFolder> feeds;

        public FeedFoldersRemovedEventArgs(IEnumerable<FeedFolder> item)
        {
            this.feeds = item;
        }

        public FeedFoldersRemovedEventArgs(FeedFolder item)
        {
            this.feeds = new List<FeedFolder>() { item };
        }

        public IEnumerable<FeedFolder> Feeds => this.feeds;
    }

    public class FeedFoldersUpdatedEventArgs : EventArgs
    {
        private readonly IEnumerable<FeedFolder> feeds;

        public FeedFoldersUpdatedEventArgs(IEnumerable<FeedFolder> item)
        {
            this.feeds = item;
        }

        public FeedFoldersUpdatedEventArgs(FeedFolder item)
        {
            this.feeds = new List<FeedFolder>() { item };
        }

        public IEnumerable<FeedFolder> Feeds => this.feeds;
    }
}