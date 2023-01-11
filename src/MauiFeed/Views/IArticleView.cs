// <copyright file="IArticleView.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using MauiFeed.Models;

namespace MauiFeed.Views
{
    /// <summary>
    /// Article View.
    /// </summary>
    public interface IArticleView
    {
        /// <summary>
        /// Gets the RSS Web View.
        /// </summary>
        IRssWebview RssWebview { get; }

        /// <summary>
        /// Set the feed item for the given article.
        /// </summary>
        /// <param name="item">The item to set.</param>
        void SetFeedItem(FeedItem item);
    }
}