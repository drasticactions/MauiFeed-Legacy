// <copyright file="FeedReaderService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using AngleSharp.Html.Parser;
using AngleSharp.Io;
using CodeHollow.FeedReader;
using Drastic.Services;
using MauiFeed.Models;
using MauiFeed.Services;
using MauiFeed.Tools;

namespace MauiFeed.NewsService
{
    /// <summary>
    /// Feed Reader Service.
    /// </summary>
    public class FeedReaderService : IRssService
    {
        private HttpClient client;
        private HtmlParser parser;
        private byte[] placeholderImage;
        private IErrorHandlerService errorHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedReaderService"/> class.
        /// </summary>
        /// <param name="errorHandler">Error handler.</param>
        /// <param name="client">Optional HttpClient.</param>
        public FeedReaderService(IErrorHandlerService errorHandler, HttpClient? client = default)
        {
            this.errorHandler = errorHandler;
            this.client = client ?? new HttpClient();
            this.parser = new HtmlParser();
            this.placeholderImage = Utilities.GetPlaceholderIcon();
        }

        /// <inheritdoc/>
        public async Task<(FeedListItem? FeedList, IList<Models.FeedItem>? FeedItemList)> ReadFeedAsync(string feedUri, FeedListItemType type = FeedListItemType.Local, CancellationToken? token = default)
        {
            var cancelationToken = token ?? CancellationToken.None;
            Feed? feed = null;
            try
            {
                feed = await FeedReader.ReadAsync(feedUri, cancelationToken);
            }
            catch (Exception ex)
            {
                this.errorHandler.HandleError(ex);
            }

            if (feed is not null)
            {
                var item = feed.ToFeedListItem(feedUri);
                item.Type = type;

                if (item.ImageCache is null)
                {
                    if (item.Uri is not null)
                    {
                        try
                        {
                            // If ImageUri is null, try to get the favicon from the site itself.
                            item.ImageCache = await this.GetFaviconFromUriAsync(item.Uri);
                        }
                        catch (Exception)
                        {
                            // If it fails to work for whatever reason, ignore it for now, use the placeholder.
                        }
                    }

                    if (!item.HasValidImage() && feed.Items.Any() && feed.Items.First().Link is string link)
                    {
                        try
                        {
                            // If ImageUri is null, try to get the favicon from the site itself.
                            item.ImageCache = await this.GetFaviconFromUriAsync(new Uri(link));
                        }
                        catch (Exception)
                        {
                            // If it fails to work for whatever reason, ignore it for now, use the placeholder.
                        }
                    }

                    if (!item.HasValidImage())
                    {
                        try
                        {
                            item.ImageCache = await this.GetByteArrayAsync(item.ImageUri!);
                        }
                        catch (Exception)
                        {
                            // If it fails to work for whatever reason, ignore it for now, use the placeholder.
                        }
                    }
                }

                // If still null, use the placeholder.
                if (!item.HasValidImage())
                {
                    item.ImageCache = this.placeholderImage;
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

        private async Task<byte[]?> GetFaviconFromUriAsync(Uri uri)
            => await this.GetByteArrayAsync(new Uri($"{uri.Scheme}://{uri.Host}/favicon.ico"));

        private async Task<byte[]?> GetByteArrayAsync(Uri uri)
        {
            using HttpResponseMessage response = await this.client.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
            {
                throw new ArgumentException("Could not get image");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
