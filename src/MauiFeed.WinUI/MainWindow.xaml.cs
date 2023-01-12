// <copyright file="MainWindow.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.UI;
using Drastic.Services;
using Drastic.Tools;
using Force.DeepCloner;
using MauiFeed.Models;
using MauiFeed.Services;
using MauiFeed.Views;
using MauiFeed.WinUI.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using static System.Net.Mime.MediaTypeNames;

namespace MauiFeed.WinUI
{
    /// <summary>
    /// The main window of the app.
    /// </summary>
    public sealed partial class MainWindow : Window, ISidebarView
    {
        private DatabaseContext databaseContext;
        private IErrorHandlerService errorHandler;
        private FeedTimelineSplitView timelineSplitView;
        private FeedNavigationViewItem? addFeedButton;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.databaseContext = Ioc.Default.ResolveWith<DatabaseContext>();
            this.errorHandler = Ioc.Default.GetService<IErrorHandlerService>()!;

            this.timelineSplitView = new FeedTimelineSplitView(this);

            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(this.AppTitleBar);
            var manager = WinUIEx.WindowManager.Get(this);
            manager.Backdrop = new WinUIEx.MicaSystemBackdrop();

            this.RemoveFeedCommand = new AsyncCommand<FeedListItem>(this.RemoveFeed, (x) => true, this.errorHandler);

            this.GenerateSidebar();

            this.NavigationFrame.Content = this.timelineSplitView;
        }

        public ObservableCollection<FeedNavigationViewItem> Items { get; set; } = new ObservableCollection<FeedNavigationViewItem>();

        public AsyncCommand<FeedListItem> RemoveFeedCommand { get; private set; }

        /// <inheritdoc/>
        public void UpdateSidebar()
        {
            foreach (var item in this.Items)
            {
                item.Update();
            }
        }

        /// <inheritdoc/>
        public void AddItemToSidebar(FeedListItem item)
        {
            var oldItem = this.Items.FirstOrDefault(n => n.FeedListItem?.Id == item.Id);

            // If we don't have this item already in the list, add it.
            // Then, update the sidebar. If you tried adding an existing item,
            // It would have updated the feed, so we can show the new items.
            if (oldItem is null)
            {
                var navItem = this.GenerateNavItem(item);
                this.Items.Add(navItem);
            }

            this.UpdateSidebar();
        }

        /// <inheritdoc/>
        public void GenerateSidebar()
        {
            this.GenerateSmartFeeds();
            this.GenerateNavItems().FireAndForgetSafeAsync(this.errorHandler);
        }

        public void GenerateSmartFeeds()
        {
            this.addFeedButton = new FeedNavigationViewItem(Translations.Common.AddFeedButton, new SymbolIcon(Symbol.Add), this.databaseContext);
            this.addFeedButton.SelectsOnInvoked = false;
            this.addFeedButton.Tapped += this.AddFeedButton_Tapped;
            this.addFeedButton.ContextFlyout = new Flyout() { Content = new AddNewFeedFlyout(this) };

            this.Items.Add(this.addFeedButton);

            var smartFilters = new FeedNavigationViewItem(Translations.Common.SmartFeedsLabel, new SymbolIcon(Symbol.Filter), this.databaseContext);
            smartFilters.SelectsOnInvoked = false;

            var all = new FeedNavigationViewItem(Translations.Common.AllLabel, new SymbolIcon(Symbol.Bookmarks), this.databaseContext, this.databaseContext.CreateFilter<FeedItem, int>(o => o.Id, 0, DatabaseContext.FilterType.GreaterThan));
            smartFilters.MenuItems.Add(all);

            var today = new FeedNavigationViewItem(Translations.Common.TodayLabel, new SymbolIcon(Symbol.GoToToday), this.databaseContext, this.databaseContext.CreateFilter<FeedItem, DateTime?>(o => o.PublishingDate, DateTime.UtcNow.Date, DatabaseContext.FilterType.GreaterThanOrEqual));
            smartFilters.MenuItems.Add(today);

            var unread = new FeedNavigationViewItem(Translations.Common.AllUnreadLabel, new SymbolIcon(Symbol.Filter), this.databaseContext, this.databaseContext.CreateFilter<FeedItem, bool>(o => o.IsRead, false, DatabaseContext.FilterType.Equals));
            smartFilters.MenuItems.Add(unread);

            var star = new FeedNavigationViewItem(Translations.Common.StarredLabel, new SymbolIcon(Symbol.Favorite), this.databaseContext, this.databaseContext.CreateFilter<FeedItem, bool>(o => o.IsFavorite, true, DatabaseContext.FilterType.Equals));
            smartFilters.MenuItems.Add(star);

            this.Items.Add(smartFilters);
        }

        private async Task GenerateNavItems()
        {
            var feedItems = await this.databaseContext.GetAllFeedListAsync();
            foreach (var feed in feedItems)
            {
                var test = this.GenerateNavItem(feed);
                this.Items.Add(test);
            }
        }

        private FeedNavigationViewItem GenerateNavItem(FeedListItem? item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var test = this.databaseContext.CreateFilter<FeedItem, int>(o => o.FeedListItemId, item.Id, DatabaseContext.FilterType.Equals);
            return new FeedNavigationViewItem(item.Name!, item, this.databaseContext, test);
        }

        /// <summary>
        /// Remove the feed item.
        /// </summary>
        /// <param name="item">The feed item to mark.</param>
        /// <returns>Task.</returns>
        public async Task RemoveFeed(FeedListItem item)
        {
            this.UpdateSidebar();
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is not FeedNavigationViewItem item)
            {
                return;
            }

            this.timelineSplitView.SetFeed(item);
        }

        private void AddFeedButton_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ((FrameworkElement)sender)!.ContextFlyout.ShowAt((FrameworkElement)sender!);
        }
    }
}
