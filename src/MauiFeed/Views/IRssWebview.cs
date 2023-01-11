// <copyright file="IRssWebview.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace MauiFeed.Views
{
    /// <summary>
    /// Rss Webview.
    /// </summary>
    public interface IRssWebview
    {
        /// <summary>
        /// Get the source for the given webview.
        /// </summary>
        /// <param name="html">The HTML to set.</param>
        void SetSource(string html);
    }
}
