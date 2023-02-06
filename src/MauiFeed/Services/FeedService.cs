// <copyright file="FeedService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using System.Xml;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using CodeHollow.FeedReader;
using Drastic.Services;
using JsonFeedNet;
using MauiFeed.Models;
using MauiFeed.Tools;
using Newtonsoft.Json.Linq;

namespace MauiFeed.Services
{
    /// <summary>
    /// Feed Service.
    /// </summary>
    public class FeedService
    {
        private HttpClient client;
        private HtmlParser parser;
        private byte[] placeholderImage;
        private IErrorHandlerService errorHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedService"/> class.
        /// </summary>
        /// <param name="errorHandler">Error handler.</param>
        /// <param name="client">Optional HttpClient.</param>
        public FeedService(IErrorHandlerService errorHandler, HttpClient? client = default)
        {
            this.errorHandler = errorHandler;
            this.client = client ?? new HttpClient();
            this.parser = new HtmlParser();
            this.placeholderImage = Utilities.GetPlaceholderIcon();
        }

        /// <summary>
        /// Read Feed Async.
        /// </summary>
        /// <param name="feedUri">Feed Item.</param>
        /// <param name="token">Token.</param>
        /// <returns>FeedList and Item.</returns>
        public async Task<(FeedListItem? FeedList, IList<Models.FeedItem>? FeedItemList)> ReadFeedAsync(string feedUri, CancellationToken? token = default)
            => await this.ReadFeedAsync(new FeedListItem() { Uri = new Uri(feedUri) }, token);

        /// <summary>
        /// Read Feed Async.
        /// </summary>
        /// <param name="feedUri">Feed Item.</param>
        /// <param name="token">Token.</param>
        /// <returns>FeedList and Item.</returns>
        public async Task<(FeedListItem? FeedList, IList<Models.FeedItem>? FeedItemList)> ReadFeedAsync(Uri feedUri, CancellationToken? token = default)
            => await this.ReadFeedAsync(new FeedListItem() { Uri = feedUri }, token);

        /// <summary>
        /// Read Feed Async.
        /// </summary>
        /// <param name="feedItem">Feed Item.</param>
        /// <param name="token">Token.</param>
        /// <returns>FeedList and Item.</returns>
        /// <exception cref="ArgumentNullException">Thrown if URI not set on feed item.</exception>
        /// <exception cref="NotImplementedException">Thrown if unknown feed type detected.</exception>
        public async Task<(FeedListItem? FeedList, IList<Models.FeedItem>? FeedItemList)> ReadFeedAsync(FeedListItem feedItem, CancellationToken? token = default)
        {
            if (feedItem.Uri is null)
            {
                throw new ArgumentNullException(nameof(feedItem));
            }

            var cancelationToken = token ?? CancellationToken.None;
            string stringResponse = string.Empty;
            try
            {
                using var response = await this.client.GetAsync(feedItem.Uri, cancelationToken);
                stringResponse = (await response.Content.ReadAsStringAsync()).Trim();
            }
            catch (Exception ex)
            {
                this.errorHandler.HandleError(ex);
            }

            // We have a response, time to figure out what to do with it.
            try
            {
                if (!string.IsNullOrEmpty(stringResponse))
                {
                    var type = feedItem.FeedType;

                    if (type == Models.FeedType.Unknown)
                    {
                        type = this.ValidateString(stringResponse);
                    }

                    System.Diagnostics.Debug.Assert(type != Models.FeedType.Unknown, "Should not have unknown at this point");

                    return type switch
                    {
                        Models.FeedType.Unknown => throw new NotImplementedException(),
                        Models.FeedType.Rss => await this.ParseWithFeedReaderAsync(stringResponse, feedItem, cancelationToken),
                        Models.FeedType.Json => await this.ParseWithJsonFeedAsync(stringResponse, feedItem, cancelationToken),
                        _ => throw new NotImplementedException(),
                    };
                }
            }
            catch (Exception ex)
            {
                this.errorHandler.HandleError(ex);
            }

            return (null, null);
        }

        private async Task<(FeedListItem? FeedList, IList<Models.FeedItem>? FeedItemList)> ParseWithFeedReaderAsync(string stringResponse, FeedListItem feedItem, CancellationToken? token = default)
        {
            var feed = FeedReader.ReadFromString(stringResponse);
            feedItem = feed.Update(feedItem);
            var items = feed.Items.Select(n => n.ToFeedItem(feedItem)).ToList();

            if (!feedItem.HasValidImage())
            {
                await this.GetImageForItem(feedItem, items);
            }

            return (feedItem, items);
        }

        private async Task<(FeedListItem? FeedList, IList<Models.FeedItem>? FeedItemList)> ParseWithJsonFeedAsync(string stringResponse, FeedListItem feedItem, CancellationToken? token = default)
        {
            var feed = JsonFeed.Parse(stringResponse);

            feedItem = feed.Update(feedItem);
            var items = feed.Items.Select(n => n.ToFeedItem(feedItem)).ToList();
            if (!feedItem.HasValidImage())
            {
                await this.GetImageForItem(feedItem, items);
            }

            return (feedItem, items);
        }

        private MauiFeed.Models.FeedType ValidateString(string stringResponse)
        {
            if (this.IsXml(stringResponse))
            {
                return MauiFeed.Models.FeedType.Rss;
            }
            else if (this.IsJson(stringResponse))
            {
                return MauiFeed.Models.FeedType.Json;
            }

            return Models.FeedType.Unknown;
        }

        private bool IsXml(string stringResponse)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(stringResponse);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsJson(string stringResponse)
        {
            try
            {
                var test = System.Text.Json.JsonSerializer.Deserialize<object>(stringResponse);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task GetImageForItem(FeedListItem item, List<Models.FeedItem> items)
        {
            if (item.ImageUri is not null)
            {
                try
                {
                    item.ImageCache = await this.GetByteArrayAsync(item.ImageUri);
                }
                catch (Exception)
                {
                    // If it fails to work for whatever reason, ignore it for now, use the placeholder.
                }
            }

            if (!item.HasValidImage() && item.Uri is not null)
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

            if (!item.HasValidImage() && items.Any() && items.First().Link is string link)
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
                    // If all else fails, get the icon from the webpage itself by parsing it.
                    item.ImageCache = await this.ParseRootWebpageForIcon(item.Uri!);
                }
                catch (Exception)
                {
                    // If it fails to work for whatever reason, ignore it for now, use the placeholder.
                }
            }
        }

        private async Task<byte[]?> GetFaviconFromUriAsync(Uri uri)
            => await this.GetByteArrayAsync(new Uri($"{uri.Scheme}://{uri.Host}/favicon.ico"));

        private async Task<byte[]?> ParseRootWebpageForIcon(Uri uri)
        {
            var htmlString = await this.client.GetStringAsync(new Uri($"{uri.Scheme}://{uri.Host}/"));
            var html = await this.parser.ParseDocumentAsync(htmlString);
            var favIcon = html.QuerySelector("link[rel~='icon']");
            if (favIcon is not IHtmlLinkElement anchor)
            {
                return null;
            }

            return anchor.Href is not null ? await this.GetByteArrayAsync(new Uri(anchor.Href)) : null;
        }

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
