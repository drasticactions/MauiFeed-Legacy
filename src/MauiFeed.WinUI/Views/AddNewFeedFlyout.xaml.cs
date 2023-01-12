// <copyright file="AddNewFeedFlyout.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Services;
using Drastic.Tools;
using MauiFeed.Services;
using MauiFeed.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace MauiFeed.WinUI.Views
{
    public sealed partial class AddNewFeedFlyout : UserControl
    {
        private RssFeedCacheService cache;
        private ISidebarView sidebar;
        private IErrorHandlerService errorHandler;

        public AddNewFeedFlyout(ISidebarView sidebar)
        {
            this.InitializeComponent();

            this.cache = (RssFeedCacheService)Ioc.Default.GetService(typeof(RssFeedCacheService))!;
            this.errorHandler = (IErrorHandlerService)Ioc.Default.GetService(typeof(IErrorHandlerService))!;
            this.sidebar = sidebar;
            this.AddNewFeedCommand = new AsyncCommand<string>(this.AddNewFeed, (x) => true, this.errorHandler);
        }

        public AsyncCommand<string> AddNewFeedCommand { get; private set; }

        public async Task AddNewFeed(string feedUri)
        {
            Uri.TryCreate(feedUri, UriKind.Absolute, out Uri? uri);
            if (uri is null)
            {
                return;
            }

            var popup = ((Popup)((FrameworkElement)this.Parent).Parent)!;
            popup.IsOpen = false;

            var feed = await this.cache.RetrieveFeedAsync(uri);
            this.sidebar.AddItemToSidebar(feed);
        }
    }
}
