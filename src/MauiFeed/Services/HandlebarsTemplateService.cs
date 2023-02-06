// <copyright file="HandlebarsTemplateService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Reflection;
using System.Text.RegularExpressions;
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
        public async Task<string> RenderFeedItemAsync(FeedItem item, bool darkMode)
        {
            var link = item.Link ?? item.ExternalLink;
            if (link is null)
            {
                throw new ArgumentNullException(nameof(link));
            }

            var isHtml = Regex.IsMatch(item.Content ?? string.Empty, "<.*?>");

            if (!isHtml)
            {
                SmartReader.Article article = await SmartReader.Reader.ParseArticleAsync(link);
                item.Html = article.Content;
            }
            else
            {
                item.Html = item.Content;
            }

            // Replace all links with blank targets, to force them to open in new tabs.
            item.Html = Regex.Replace(item.Html ?? string.Empty, "<(a)([^>]+)>", @"<$1 target=""_blank""$2>");

            return this.feedItemTemplate.Invoke(new { IsDarkMode = darkMode, FeedListItem = item.Feed, FeedItem = item, Image = Convert.ToBase64String(item.Feed?.ImageCache ?? new byte[0]) });
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
