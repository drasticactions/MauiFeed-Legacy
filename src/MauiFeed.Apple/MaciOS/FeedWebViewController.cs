// <copyright file="FeedWebViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.PureLayout;
using Drastic.Tools;
using MauiFeed.Models;
using MauiFeed.Services;
using MauiFeed.Views;

namespace MauiFeed.Apple
{
    public class FeedWebViewController : UIViewController, IArticleView
    {
        private ITemplateService templateService;

        private RssWebview webview;

        private RootSplitViewController rootSplitViewController;

        public FeedWebViewController(RootSplitViewController controller)
        {
            this.rootSplitViewController = controller;
            this.templateService = Ioc.Default.GetService<ITemplateService>()!;
            this.webview = new RssWebview(this.View?.Frame ?? CGRect.Empty, new WebKit.WKWebViewConfiguration());
            this.View?.AddSubview(this.webview);
            this.webview.AutoPinEdgesToSuperviewSafeArea();
        }

        public IRssWebview RssWebview => throw new NotImplementedException();

        public void SetFeedItem(FeedItem item)
        {

/* プロジェクト 'MauiFeed.Apple(net7.0-maccatalyst)' からのマージされていない変更
前:
            Task.Run(async () => {
後:
            Task.Run(async () =>
            {
*/
            Task.Run(async () =>
            {
                var result = await this.templateService.RenderFeedItemAsync(item, true);
                this.webview.SetSource(result);
            }).FireAndForgetSafeAsync();
        }
    }
}