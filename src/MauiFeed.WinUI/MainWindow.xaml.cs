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
using MauiFeed.WinUI.Services;
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
    public sealed partial class MainWindow : WinUIEx.WindowEx, ISidebarView
    {
        private DatabaseContext databaseContext;
        private IErrorHandlerService errorHandler;
        private FeedTimelineSplitView timelineSplitView;
        private FeedNavigationViewItem? addFeedButton;
        private FeedNavigationViewItem? allButton;
        private RssFeedCacheService rssFeedCacheService;
        private Progress<RssCacheFeedUpdate> progressUpdate;
        private SettingsPage settingsPage;
        private ThemeSelectorService themeSelector;
        private FeedNavigationViewItem? selectedNavItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.databaseContext = Ioc.Default.ResolveWith<DatabaseContext>();
            this.errorHandler = Ioc.Default.GetService<IErrorHandlerService>()!;
            this.rssFeedCacheService = Ioc.Default.GetService<RssFeedCacheService>()!;
            this.progressUpdate = new Progress<RssCacheFeedUpdate>();
            this.progressUpdate.ProgressChanged += ProgressUpdate_ProgressChanged;
            this.themeSelector = new ThemeSelectorService(this);
            this.timelineSplitView = new FeedTimelineSplitView(this, this.themeSelector);
            this.settingsPage = new SettingsPage(this.themeSelector);
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(this.AppTitleBar);
            var manager = WinUIEx.WindowManager.Get(this);
            manager.Backdrop = new WinUIEx.MicaSystemBackdrop();

            this.GenerateSidebar();

            this.NavigationFrame.Content = this.timelineSplitView;
            this.themeSelector.SetRequestedTheme();
        }

        public ObservableCollection<FeedNavigationViewItem> Items { get; set; } = new ObservableCollection<FeedNavigationViewItem>();

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
            var refreshButton = new FeedNavigationViewItem(Translations.Common.RefreshButton, new SymbolIcon(Symbol.Refresh), this.databaseContext);
            refreshButton.SelectsOnInvoked = false;
            refreshButton.Tapped += this.RefreshButton_Tapped;
            this.Items.Add(refreshButton);

            this.addFeedButton = new FeedNavigationViewItem(Translations.Common.AddFeedButton, new SymbolIcon(Symbol.Add), this.databaseContext);
            this.addFeedButton.SelectsOnInvoked = false;
            this.addFeedButton.Tapped += this.AddFeedButton_Tapped;
            this.addFeedButton.ContextFlyout = new Flyout() { Content = new AddNewFeedFlyout(this) };

            this.Items.Add(this.addFeedButton);

            var smartFilters = new FeedNavigationViewItem(Translations.Common.SmartFeedsLabel, new SymbolIcon(Symbol.Filter), this.databaseContext);
            smartFilters.SelectsOnInvoked = false;

            this.allButton = new FeedNavigationViewItem(Translations.Common.AllLabel, new SymbolIcon(Symbol.Bookmarks), this.databaseContext, this.databaseContext.CreateFilter<FeedItem, int>(o => o.Id, 0, DatabaseContext.FilterType.GreaterThan));
            smartFilters.MenuItems.Add(this.allButton);

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
            var navItem = new FeedNavigationViewItem(item.Name!, item, this.databaseContext, test);
            var command = new AsyncCommand<FeedNavigationViewItem>(this.RemoveFeed, (item) => true, this.errorHandler);
            navItem.ContextFlyout = new Flyout() { Content = new RemoveFeedFlyout(command, navItem) };
            navItem.RightTapped += (sender, args) =>
            {
                ((FrameworkElement)sender)!.ContextFlyout.ShowAt((FrameworkElement)sender!);
            };
            return navItem;
        }

        /// <summary>
        /// Remove the feed item.
        /// </summary>
        /// <param name="item">The feed item to mark.</param>
        /// <returns>Task.</returns>
        public async Task RemoveFeed(FeedNavigationViewItem item)
        {
            // If you're removing the item you have selected, reset the feed to All.
            if (this.NavView.SelectedItem == item)
            {
                this.NavView.SelectedItem = this.allButton;
            }

            this.Items.Remove(item);
            await this.databaseContext.RemoveFeedListItem(item.FeedListItem!);
            this.UpdateSidebar();
            this.timelineSplitView.UpdateFeed();
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is not FeedNavigationViewItem item)
            {
                if (args.SelectedItem is NavigationViewItem nav)
                {
                    if (nav.Tag.ToString() == "Settings")
                    {
                        this.NavigationFrame.Content = this.settingsPage;
                    }

                    return;
                }

                return;
            }

            this.NavigationFrame.Content = this.timelineSplitView;
            this.timelineSplitView.SetFeed(item);
        }

        private void AddFeedButton_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ((FrameworkElement)sender)!.ContextFlyout.ShowAt((FrameworkElement)sender!);
        }

        private async void RefreshButton_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await this.rssFeedCacheService.RefreshFeedsAsync(this.progressUpdate);
            this.timelineSplitView.UpdateFeed();
        }

        private void ProgressUpdate_ProgressChanged(object? sender, RssCacheFeedUpdate e)
        {
            // TODO: Show UI for updating each item.
            // This is contained in RssCacheFeedUpdate.
            if (e.IsDone)
            {
                this.UpdateSidebar();
            }
        }
    }
}
