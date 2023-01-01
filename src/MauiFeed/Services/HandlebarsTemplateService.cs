// <copyright file="HandlebarsTemplateService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Reflection;
using HandlebarsDotNet;
using MauiFeed.Models;

namespace MauiFeed.Services
{
    /// <summary>
    /// Handlebars Template Service.
    /// </summary>
    public class HandlebarsTemplateService : ITemplateService
    {
        private HandlebarsTemplate<object, object> feedItemTemplate;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlebarsTemplateService"/> class.
        /// </summary>
        public HandlebarsTemplateService()
        {
            this.feedItemTemplate = Handlebars.Compile(HandlebarsTemplateService.GetResourceFileContentAsString("Templates.feeditem.html.hbs"));
        }

        /// <inheritdoc/>
        public async Task<string> RenderFeedItemAsync(FeedListItem feedListItem, FeedItem item)
        {
            if (item.Link is null)
            {
                throw new ArgumentNullException(nameof(item.Link));
            }

            SmartReader.Article article = await SmartReader.Reader.ParseArticleAsync(item.Link);
            item.Html = article.Content;
            return this.feedItemTemplate.Invoke(new { FeedListItem = feedListItem, FeedItem = item, Image = Convert.ToBase64String(feedListItem.ImageCache ?? new byte[0]) });
        }

        private static string GetResourceFileContentAsString(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            if (assembly is null)
            {
                return string.Empty;
            }

            var resourceName = "MauiFeed." + fileName;

            string? resource = null;
            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream is null)
                {
                    return string.Empty;
                }

                using StreamReader reader = new StreamReader(stream);
                resource = reader.ReadToEnd();
            }

            return resource ?? string.Empty;
        }
    }
}
