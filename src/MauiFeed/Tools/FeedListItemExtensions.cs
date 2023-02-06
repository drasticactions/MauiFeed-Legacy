// <copyright file="FeedListItemExtensions.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Web;
using System.Xml.Linq;
using CodeHollow.FeedReader;
using JsonFeedNet;
using MauiFeed.Models;
using MauiFeed.Models.OPML;

namespace MauiFeed
{
    /// <summary>
    /// Feed List Item Extensions.
    /// </summary>
    public static class FeedListItemExtensions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedListItem"/> class.
        /// </summary>
        /// <param name="feed"><see cref="Feed"/>.</param>
        /// <param name="feedUri">Original Feed Uri.</param>
        /// <returns><see cref="FeedListItem"/>.</returns>
        public static FeedListItem ToFeedListItem(this Feed feed, string feedUri)
        {
            return new FeedListItem()
            {
                Name = feed.Title,
                Uri = new Uri(feedUri),
                Link = feed.Link,
                ImageUri = string.IsNullOrEmpty(feed.ImageUrl) ? null : new Uri(feed.ImageUrl),
                Description = feed.Description,
                Language = feed.Language,
                LastUpdatedDate = feed.LastUpdatedDate,
                LastUpdatedDateString = feed.LastUpdatedDateString,
                FeedType = Models.FeedType.Rss,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedListItem"/> class.
        /// </summary>
        /// <param name="feed"><see cref="Feed"/>.</param>
        /// <param name="oldItem">Original Feed Uri.</param>
        /// <returns><see cref="FeedListItem"/>.</returns>
        public static FeedListItem Update(this Feed feed, FeedListItem oldItem)
        {
            oldItem.Name = feed.Title;
            oldItem.Link = feed.Link;
            oldItem.ImageUri = string.IsNullOrEmpty(feed.ImageUrl) ? null : new Uri(feed.ImageUrl);
            oldItem.Description = feed.Description;
            oldItem.Language = feed.Language;
            oldItem.LastUpdatedDate = feed.LastUpdatedDate;
            oldItem.LastUpdatedDateString = feed.LastUpdatedDateString;
            oldItem.FeedType = Models.FeedType.Rss;
            return oldItem;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedListItem"/> class.
        /// </summary>
        /// <param name="feed"><see cref="Feed"/>.</param>
        /// <param name="feedUri">Original Feed Uri.</param>
        /// <returns><see cref="FeedListItem"/>.</returns>
        public static FeedListItem ToFeedListItem(this JsonFeed feed, string feedUri)
        {
            return new FeedListItem()
            {
                Name = feed.Title,
                Uri = new Uri(feedUri),
                Link = feed.HomePageUrl,
                ImageUri = string.IsNullOrEmpty(feed.Icon) ? null : new Uri(feed.Icon),
                Description = feed.Description,
                Language = feed.Language,
                FeedType = Models.FeedType.Json,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedListItem"/> class.
        /// </summary>
        /// <param name="feed"><see cref="Feed"/>.</param>
        /// <param name="oldItem">Old Item.</param>
        /// <returns><see cref="FeedListItem"/>.</returns>
        public static FeedListItem Update(this JsonFeed feed, FeedListItem oldItem)
        {
            oldItem.Name = feed.Title;
            oldItem.Link = feed.HomePageUrl;
            oldItem.ImageUri = string.IsNullOrEmpty(feed.Icon) ? null : new Uri(feed.Icon);
            oldItem.Description = feed.Description;
            oldItem.Language = feed.Language;
            oldItem.FeedType = Models.FeedType.Json;
            return oldItem;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedListItem"/> class.
        /// </summary>
        /// <param name="feed"><see cref="Outline"/>.</param>
        /// <returns><see cref="FeedListItem"/>.</returns>
        public static FeedListItem ToFeedListItem(this Outline feed)
        {
            return new FeedListItem()
            {
                Name = feed.Title,
                Uri = feed.XMLUrl is not null ? new Uri(feed.XMLUrl) : null,
                Link = feed.HTMLUrl,
                ImageUri = null,
                Description = feed.Description,
                Language = feed.Language,
                Folder = feed.Parent is not null ? new FeedFolder() { Name = feed.Parent!.Title } : null,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedItem"/> class.
        /// </summary>
        /// <param name="item"><see cref="CodeHollow.FeedReader.FeedItem"/>.</param>
        /// <param name="feedListItem"><see cref="FeedListItem"/>.</param>
        /// <param name="imageUrl">Image Url.</param>
        /// <returns><see cref="FeedItem"/>.</returns>
        public static Models.FeedItem ToFeedItem(this CodeHollow.FeedReader.FeedItem item, FeedListItem feedListItem, string? imageUrl = "")
        {
            var content = HttpUtility.HtmlDecode(item.Content);
            var description = HttpUtility.HtmlDecode(item.Description);
            return new Models.FeedItem()
            {
                RssId = item.Id,
                FeedListItemId = feedListItem.Id,
                Title = item.Title,
                Link = item.Link,
                Description = description,
                PublishingDate = item.PublishingDate,
                Author = item.Author,
                Content = content,
                PublishingDateString = item.PublishingDateString,
                ImageUrl = imageUrl,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedItem"/> class.
        /// </summary>
        /// <param name="item"><see cref="CodeHollow.FeedReader.FeedItem"/>.</param>
        /// <param name="feedListItem"><see cref="FeedListItem"/>.</param>
        /// <param name="imageUrl">Image Url.</param>
        /// <returns><see cref="FeedItem"/>.</returns>
        public static Models.FeedItem ToFeedItem(this JsonFeedItem item, FeedListItem feedListItem, string? imageUrl = "")
        {
            var authors = string.Empty;
            if (item.Authors is not null)
            {
                authors = string.Join(", ", item.Authors.Select(n => n.Name));
            }

            var content = item.ContentHtml ?? item.ContentText;

            return new Models.FeedItem()
            {
                RssId = item.Id,
                FeedListItemId = feedListItem.Id,
                Title = item.Title,
                Link = item.Url,
                ExternalLink = item.ExternalUrl,
                Description = string.Empty,
                PublishingDate = item.DatePublished?.DateTime,
                Author = authors,
                Content = HttpUtility.HtmlDecode(content),
                PublishingDateString = item.DatePublished.ToString(),
                ImageUrl = imageUrl,
            };
        }
    }
}
