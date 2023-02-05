// <copyright file="OpmlFeedListItemFactory.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using AngleSharp.Dom;
using MauiFeed.Models;
using MauiFeed.Models.OPML;
using MauiFeed.Tools;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="OpmlFeedListItemFactory"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="client">Http Client.</param>
        public OpmlFeedListItemFactory(DatabaseContext context, HttpClient? client = default)
        {
            this.context = context;
            this.client = client ?? new HttpClient();
            this.placeholderImage = Utilities.GetPlaceholderIcon();
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
            if (item.Uri is not null)
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

            // If still null, use the placeholder.
            if (!item.HasValidImage())
            {
                item.ImageCache = this.placeholderImage;
            }
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