// <copyright file="AddNewFeedFlyout.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Services;
using Drastic.Tools;
using MauiFeed.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace MauiFeed.WinUI.Views
{
    /// <summary>
    /// Add New Feed Flyout.
    /// </summary>
    public sealed partial class AddNewFeedFlyout : UserControl
    {
        private RssFeedCacheService cache;
        private MainWindow sidebar;
        private IErrorHandlerService errorHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddNewFeedFlyout"/> class.
        /// </summary>
        /// <param name="sidebar">Sidebar.</param>
        public AddNewFeedFlyout(MainWindow sidebar)
        {
            this.InitializeComponent();
            this.cache = (RssFeedCacheService)Ioc.Default.GetService(typeof(RssFeedCacheService))!;
            this.errorHandler = (IErrorHandlerService)Ioc.Default.GetService(typeof(IErrorHandlerService))!;
            this.sidebar = sidebar;
            this.AddNewFeedCommand = new AsyncCommand<string>(this.AddNewFeed, (x) => true, this.errorHandler);
        }

        /// <summary>
        /// Gets the Add New Feed Command.
        /// </summary>
        public AsyncCommand<string> AddNewFeedCommand { get; private set; }

        private async Task AddNewFeed(string feedUri)
        {
            Uri.TryCreate(feedUri, UriKind.Absolute, out Uri? uri);
            if (uri is null)
            {
                return;
            }

            var popup = ((Popup)((FrameworkElement)this.Parent).Parent)!;
            popup.IsOpen = false;

            if (!this.sidebar.SidebarItems.Any(n => n.FeedListItem?.Uri == uri))
            {
                var feed = await this.cache.RetrieveFeedAsync(uri);
                if (feed is not null)
                {
                    this.sidebar.AddItemToSidebar(feed);
                }
            }

            this.FeedUrlField.Text = string.Empty;
        }

        private void FeedUrlField_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (string.IsNullOrEmpty(this.FeedUrlField.Text))
                {
                    return;
                }

                this.AddNewFeedCommand.ExecuteAsync(this.FeedUrlField.Text).FireAndForgetSafeAsync();
            }
        }
    }
}
