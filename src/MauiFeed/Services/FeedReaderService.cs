// <copyright file="FeedReaderService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using AngleSharp.Html.Parser;
using CodeHollow.FeedReader;
using MauiFeed.Models;

namespace MauiFeed.Services
{
    /// <summary>
    /// Feed Reader Service.
    /// </summary>
    public class FeedReaderService : IRssService
    {
        private HttpClient client;
        private HtmlParser parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedReaderService"/> class.
        /// </summary>
        public FeedReaderService(HttpClient? client = default)
        {
            this.client = client ?? new HttpClient();
            this.parser = new HtmlParser();
        }

        /// <inheritdoc/>
        public async Task<(FeedListItem? FeedList, IList<Models.FeedItem>? FeedItemList)> ReadFeedAsync(string feedUri, FeedListItemType type = FeedListItemType.Local, CancellationToken? token = default)
        {
            var cancelationToken = token ?? CancellationToken.None;
            var feed = await FeedReader.ReadAsync(feedUri, cancelationToken);
            if (feed is not null)
            {
                var item = feed.ToFeedListItem(feedUri);
                item.Type = type;

                if (item.ImageCache is null && item.ImageUri is not null)
                {
                    item.ImageCache = await this.client.GetByteArrayAsync(item.ImageUri);
                }
                else if (item.ImageCache is null)
                {
                    item.ImageCache = Utilities.GetPlaceholderIcon();
                }

                var feedItemList = new List<Models.FeedItem>();

                foreach (var feedItem in feed.Items)
                {
                    using var document = await this.parser.ParseDocumentAsync(feedItem.Content);
                    var image = document.QuerySelector("img");
                    var imageUrl = string.Empty;
                    if (image is not null)
                    {
                        imageUrl = image.GetAttribute("src");
                    }

                    feedItemList.Add(feedItem.ToFeedItem(item, imageUrl));
                }

                return (item, feedItemList);
            }

            return (null, null);
        }
    }
}
