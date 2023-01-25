// <copyright file="RssWebview.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

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

        protected internal RssWebview(NativeHandle handle)
            : base(handle)
        {
        }

        protected RssWebview(NSObjectFlag t)
            : base(t)
        {
        }

        /// <inheritdoc/>
        public void SetSource(string html)
        {
            this.InvokeOnMainThread(() => this.LoadHtmlString(new NSString(html), null));
        }
    }
}