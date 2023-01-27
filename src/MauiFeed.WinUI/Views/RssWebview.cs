// <copyright file="RssWebview.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Microsoft.UI.Xaml.Controls;

namespace MauiFeed.WinUI.Views
{
    /// <summary>
    /// Rss Webview Reader.
    /// </summary>
    public class RssWebview : WebView2
    {
        /// <summary>
        /// Sets the source of the webview.
        /// </summary>
        /// <param name="html">Html.</param>
        public void SetSource(string html)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                await this.EnsureCoreWebView2Async();
                this.NavigateToString(html);
            });
        }
    }
}
