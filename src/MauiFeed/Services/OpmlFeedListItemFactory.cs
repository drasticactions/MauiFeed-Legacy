// <copyright file="OpmlFeedListItemFactory.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;
using MauiFeed.Models.OPML;

namespace MauiFeed.Services
{
    /// <summary>
    /// Opml Feed List Item Factory.
    /// </summary>
    public class OpmlFeedListItemFactory
    {
        private HttpClient client;
        private DatabaseContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpmlFeedListItemFactory"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="client">Http Client.</param>
        public OpmlFeedListItemFactory(DatabaseContext context, HttpClient? client = default)
        {
            this.context = context;
            this.client = client ?? new HttpClient();
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
                    item.ImageCache = await this.GetFaviconFromUriAsync(item.Uri);
                }
                catch (Exception)
                {
                    // If it fails to work for whatever reason, ignore it for now, use the placeholder.
                }

                if (item.ImageCache is null && item.ImageUri is not null)
                {
                    item.ImageCache = await this.client.GetByteArrayAsync(item.ImageUri);
                }
            }

            // If still null, use the placeholder.
            if (item.ImageCache is null)
            {
                item.ImageCache = Utilities.GetPlaceholderIcon();
            }
        }

        private async Task<byte[]?> GetFaviconFromUriAsync(Uri uri)
        {
            try
            {
                return await this.client.GetByteArrayAsync($"{uri.Scheme}://{uri.Host}/favicon.ico");
            }
            catch (Exception)
            {
                return null;
            }
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
