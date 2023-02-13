// <copyright file="FeedWebViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.PureLayout;
using Drastic.Tools;
using MauiFeed.Models;
using MauiFeed.Services;

namespace MauiFeed.MacCatalyst.ViewControllers
{
    /// <summary>
    /// Feed Web View Controller.
    /// </summary>
    public class FeedWebViewController : UIViewController
    {
        private ITemplateService templateService;

        private RssWebview webview;

        private RootSplitViewController rootSplitViewController;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedWebViewController"/> class.
        /// </summary>
        /// <param name="controller">Root Split View Controller.</param>
        public FeedWebViewController(RootSplitViewController controller)
        {
            this.rootSplitViewController = controller;
            this.templateService = Ioc.Default.GetService<ITemplateService>()!;
            this.webview = new RssWebview(this.View?.Frame ?? CGRect.Empty, new WebKit.WKWebViewConfiguration());
            this.View?.AddSubview(this.webview);
            this.webview.AutoPinEdgesToSuperviewSafeArea();
        }

        /// <summary>
        /// Set the feed item on the webview.
        /// </summary>
        /// <param name="item">Item.</param>
        public void SetFeedItem(FeedItem item)
        {
            Task.Run(async () =>
            {
                var result = await this.templateService.RenderFeedItemAsync(item, true);
                this.webview.SetSource(result);
            }).FireAndForgetSafeAsync();
        }
    }
}