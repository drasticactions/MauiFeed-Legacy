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
        private FeedNavigationViewItem? allButton;
        private FeedNavigationViewItem? addFolderButton;
        private RssFeedCacheService rssFeedCacheService;
        private Progress<RssCacheFeedUpdate> progressUpdate;
        private SettingsPage settingsPage;
        private ThemeSelectorService themeSelector;
        private FeedNavigationViewItem? selectedNavItem;
        private AsyncCommand<FeedNavigationViewItem> removeFeedCommand;
        private AsyncCommand<FeedNavigationViewItem> addOrUpdateFeedFolderCommand;
        private AsyncCommand<FeedNavigationViewItem> removeFeedFolderCommand;
        private FeedNavigationViewItem? smartFilters;

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
            this.removeFeedCommand = new AsyncCommand<FeedNavigationViewItem>(this.RemoveFeed, (item) => true, this.errorHandler);
            this.addOrUpdateFeedFolderCommand = new AsyncCommand<FeedNavigationViewItem>(this.AddOrUpdateFeedFolder, (item) => true, this.errorHandler);
            this.removeFeedFolderCommand = new AsyncCommand<FeedNavigationViewItem>(this.RemoveFeedFolder, (item) => true, this.errorHandler);
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
            this.GenerateFolderItems();
            this.GenerateNavItems().FireAndForgetSafeAsync(this.errorHandler);
        }

        public void GenerateSmartFeeds()
        {
            var refreshButton = new FeedNavigationViewItem(Translations.Common.RefreshButton, new SymbolIcon(Symbol.Refresh), this.databaseContext);
            refreshButton.SelectsOnInvoked = false;
            refreshButton.Tapped += this.RefreshButton_Tapped;
            this.Items.Add(refreshButton);

            var addButton = new FeedNavigationViewItem(Translations.Common.AddLabel, new SymbolIcon(Symbol.Add), this.databaseContext);
            addButton.SelectsOnInvoked = false;

            var addFeedButton = new FeedNavigationViewItem(Translations.Common.AddFeedButton, new SymbolIcon(Symbol.Library), this.databaseContext);
            addFeedButton.SelectsOnInvoked = false;
            addFeedButton.Tapped += this.AddFeedButton_Tapped;
            addFeedButton.ContextFlyout = new Flyout() { Content = new AddNewFeedFlyout(this) };

            addButton.MenuItems.Add(addFeedButton);

            this.addFolderButton = new FeedNavigationViewItem(Translations.Common.AddFolderLabel, new SymbolIcon(Symbol.NewFolder), this.databaseContext);
            this.addFolderButton.SelectsOnInvoked = false;
            this.addFolderButton.Tapped += AddFolderButton_Tapped;

            addButton.MenuItems.Add(addFolderButton);

            this.Items.Add(addButton);

            smartFilters = new FeedNavigationViewItem(Translations.Common.SmartFeedsLabel, new SymbolIcon(Symbol.Filter), this.databaseContext);
            smartFilters.SelectsOnInvoked = false;

            this.allButton = new FeedNavigationViewItem(Translations.Common.AllLabel, new SymbolIcon(Symbol.Bookmarks), this.databaseContext, this.databaseContext.CreateFilter<FeedItem, int>(o => o.Id, 0, DatabaseContext.FilterType.GreaterThan), SidebarItemType.SmartFilter);
            smartFilters.MenuItems.Add(this.allButton);

            var today = new FeedNavigationViewItem(Translations.Common.TodayLabel, new SymbolIcon(Symbol.GoToToday), this.databaseContext, this.databaseContext.CreateFilter<FeedItem, DateTime?>(o => o.PublishingDate, DateTime.UtcNow.Date, DatabaseContext.FilterType.GreaterThanOrEqual), SidebarItemType.SmartFilter);
            smartFilters.MenuItems.Add(today);

            var unread = new FeedNavigationViewItem(Translations.Common.AllUnreadLabel, new SymbolIcon(Symbol.Filter), this.databaseContext, this.databaseContext.CreateFilter<FeedItem, bool>(o => o.IsRead, false, DatabaseContext.FilterType.Equals), SidebarItemType.SmartFilter);
            smartFilters.MenuItems.Add(unread);

            var star = new FeedNavigationViewItem(Translations.Common.StarredLabel, new SymbolIcon(Symbol.Favorite), this.databaseContext, this.databaseContext.CreateFilter<FeedItem, bool>(o => o.IsFavorite, true, DatabaseContext.FilterType.Equals), SidebarItemType.SmartFilter);
            smartFilters.MenuItems.Add(star);

            this.Items.Add(smartFilters);
        }

        public void GenerateFolderItems()
        {
            foreach (var item in this.databaseContext.FeedFolder!)
            {
                this.Items.Add(this.GenerateFolderItem(item));
            }
        }

        private FeedNavigationViewItem GenerateFolderItem(FeedFolder item)
        {
            var folder = new FeedNavigationViewItem(item.Name!, new SymbolIcon(Symbol.Folder), this.databaseContext, itemType: SidebarItemType.Folder);
            folder.SelectsOnInvoked = false;
            folder.ContextFlyout = new Flyout() { Content = new FolderOptionsFlyout(this.addOrUpdateFeedFolderCommand, this.removeFeedFolderCommand, folder, item) };
            folder.Tapped += this.AddFeedButton_Tapped;
            return folder;
        }

        private async Task AddOrUpdateFeedFolder(FeedNavigationViewItem feedFolderFlyout)
        {
            if (feedFolderFlyout.ContextFlyout is null)
            {
                return;
            }

            feedFolderFlyout.ContextFlyout.Hide();
            var flyout = (FolderOptionsFlyout)((Flyout)feedFolderFlyout.ContextFlyout).Content!;

            var feedFolder = flyout.Folder;

            if (string.IsNullOrEmpty(feedFolder.Name))
            {
                return;
            }

            if (feedFolder.Id <= 0)
            {
                await this.databaseContext.AddFeedFolder(feedFolder);
                var lastsearchitem = this.Items.LastOrDefault(n => n.ItemType == SidebarItemType.Folder);
                var index = lastsearchitem is null ? this.Items.IndexOf(this.smartFilters!) : this.Items.IndexOf(lastsearchitem);
                this.Items.Insert(index + 1, this.GenerateFolderItem(feedFolder));
            }
            else
            {
                await this.databaseContext.UpdateFeedFolder(feedFolder);
                feedFolderFlyout.Content = feedFolder.Name;
            }
        }

        private async Task RemoveFeedFolder(FeedNavigationViewItem feedFolderFlyout)
        {
            var flyout = (FolderOptionsFlyout)((Flyout)feedFolderFlyout.ContextFlyout).Content!;
            var feedFolder = flyout.Folder;
            this.databaseContext.RemovedFeedFolder(feedFolder).FireAndForgetSafeAsync();
            this.Items.Remove(feedFolderFlyout);
        }

        private async Task GenerateNavItems()
        {
            var feedItems = await this.databaseContext.GetAllFeedListAsync();
            foreach (var feed in feedItems)
            {
                var test = this.GenerateNavItem(feed);
                test.CanDrag = true;
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
            var command = this.removeFeedCommand;
            navItem.ContextFlyout = new Flyout() { Content = new FeedOptionsFlyout(command, navItem) };
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

        private void AddFolderButton_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (this.addFolderButton is not null)
            {
                this.addFolderButton.ContextFlyout = new Flyout() { Content = new FolderOptionsFlyout(this.addOrUpdateFeedFolderCommand, this.removeFeedFolderCommand, this.addFolderButton) };
                ((FrameworkElement)sender)!.ContextFlyout.ShowAt((FrameworkElement)sender!);
            }
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
