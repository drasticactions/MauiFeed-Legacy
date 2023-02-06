// <copyright file="OpmlFeedListItemFactory.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using MauiFeed.Models;
using MauiFeed.Models.OPML;
using MauiFeed.Tools;
using Microsoft.EntityFrameworkCore;

namespace MauiFeed.Services
{
    /// <summary>
    /// Opml Feed List Item Factory.
    /// </summary>
    public class OpmlFeedListItemFactory
    {
        private HttpClient client;
        private DatabaseContext context;
        private byte[] placeholderImage;
        private HtmlParser parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpmlFeedListItemFactory"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="client">Http Client.</param>
        public OpmlFeedListItemFactory(DatabaseContext context, HttpClient? client = default)
        {
            this.context = context;
            this.client = client ?? new HttpClient();
            this.parser = new HtmlParser();
            this.placeholderImage = Utilities.GetPlaceholderIcon();
        }

        /// <summary>
        /// Generate an Opml feed from given database.
        /// </summary>
        /// <returns>Opml.</returns>
        public Opml GenerateOpmlFeed()
        {
            var opml = new Opml();
            opml.Head = new Head()
            {
                Title = "MauiFeed",
                DateCreated = DateTime.UtcNow,
            };

            opml.Body = new Body();

            foreach (var folder in this.context.FeedFolder!.Include(n => n.Items))
            {
                var folderOutline = new Outline() { Text = folder.Name, Title = folder.Name };
                foreach (var feedItem in folder.Items ?? new List<FeedListItem>())
                {
                    var feedOutline = new Outline() { Text = feedItem.Name, Title = feedItem.Name, Description = feedItem.Description, Type = "rss", Version = "RSS", HTMLUrl = feedItem.Link, XMLUrl = feedItem.Uri?.ToString() };
                    folderOutline.Outlines.Add(feedOutline);
                }

                opml.Body.Outlines.Add(folderOutline);
            }

            foreach (var feedItem in this.context.FeedListItems!.Where(n => n.FolderId == null))
            {
                var feedOutline = new Outline() { Text = feedItem.Name, Title = feedItem.Name, Description = feedItem.Description, Type = "rss", Version = "RSS", HTMLUrl = feedItem.Link, XMLUrl = feedItem.Uri?.ToString() };
                opml.Body.Outlines.Add(feedOutline);
            }

            return opml;
        }

        /// <summary>
        /// Generate a list of Feed List Items from Opml documents.
        /// </summary>
        /// <param name="opml">Opml.</param>
        /// <returns>Task.</returns>
        public async Task<int> GenerateFeedListItemsFromOpmlAsync(Opml opml)
        {
            var opmlGroup = opml.Body.Outlines.SelectMany(n => this.Flatten(n)).Where(n => n.IsFeed).Select(n => n.ToFeedListItem()).ToList();
            var cachedFolders = new List<FeedFolder>();
            var cachedItems = new List<FeedListItem>();

            foreach (var item in opmlGroup)
            {
                // If the item is already in the database, skip it.
                if (this.context.FeedListItems!.Any(n => n.Uri == item.Uri))
                {
                    continue;
                }

                if (item.Folder is not null)
                {
                    var cachedFolder = cachedFolders.FirstOrDefault(n => n.Name == item.Folder.Name);
                    if (cachedFolder is not null)
                    {
                        item.Folder = cachedFolder;
                    }
                    else
                    {
                        var existingFolder = this.context.FeedFolder!.FirstOrDefault(n => n.Name == item.Folder.Name);
                        if (existingFolder is not null)
                        {
                            item.Folder = existingFolder;
                        }
                        else
                        {
                            this.context.FeedFolder!.Add(item.Folder);
                            cachedFolders.Add(item.Folder);
                        }
                    }
                }

                cachedItems.Add(item);
                this.context.FeedListItems!.Add(item);
            }

            // Get all of the icons for new items.
            await Parallel.ForEachAsync(cachedItems, async (item, token) =>
            {
                await this.GetImageAsync(item);
            });

            return this.context.SaveChanges();
        }

        private async Task GetImageAsync(FeedListItem item)
        {
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

            if (!item.HasValidImage() && item.Uri is not null)
            {
                try
                {
                    // If ImageUri is null, try to get the favicon from the site itself.
                    var test = $"{item.Uri.Scheme}://{item.Uri.Host}/favicon.ico";
                    item.ImageCache = await this.GetByteArrayAsync(new Uri(test));
                }
                catch (Exception)
                {
                    // If it fails to work for whatever reason, ignore it for now, use the placeholder.
                }

                if (!item.HasValidImage() && item.ImageUri is not null)
                {
                    item.ImageCache = await this.GetByteArrayAsync(item.ImageUri);
                }
            }
        }

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

        private IEnumerable<Outline> Flatten(Outline forum)
        {
            yield return forum;
            if (forum.Outlines != null)
            {
                var forums = forum.Outlines;
                foreach (var child in forum.Outlines)
                {
                    foreach (var descendant in this.Flatten(child))
                    {
                        yield return descendant;
                    }
                }
            }
        }
    }
}