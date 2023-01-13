using System;
using MauiFeed.Views;
using ObjCRuntime;
using WebKit;

namespace MauiFeed.Apple
{
    public class RssWebview : WKWebView, IRssWebview
    {
        public RssWebview(NSCoder coder)
            : base(coder)
        {
        }

        public RssWebview(CGRect frame, WKWebViewConfiguration configuration)
            : base(frame, configuration)
        {
        }

        protected RssWebview(NSObjectFlag t)
            : base(t)
        {
        }

        protected internal RssWebview(NativeHandle handle)
            : base(handle)
        {
        }

        /// <inheritdoc/>
        public void SetSource(string html)
        {
            this.InvokeOnMainThread(() => this.LoadHtmlString(new NSString(html), null));
        }
    }
}