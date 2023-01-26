// <copyright file="ITemplateService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;

namespace MauiFeed.Services
{
    /// <summary>
    /// Template Service.
    /// </summary>
    public interface ITemplateService
    {
        /// <summary>
        /// Render Feed Item.
        /// </summary>
        /// <param name="item">FeedItem.</param>
        /// <param name="darkMode">Render feed in dark mode.</param>
        /// <returns>Html String.</returns>
        public Task<string> RenderFeedItemAsync(FeedItem item, bool darkMode);
    }
}
