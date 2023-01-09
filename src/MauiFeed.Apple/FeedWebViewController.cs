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

        public void SetFeedItem(FeedItem item)
        {
            Task.Run(async () => {
                var result = await this.templateService.RenderFeedItemAsync(item.Feed!, item);
                this.webview.SetSource(result);
            }).FireAndForgetSafeAsync();
        }
    }
}