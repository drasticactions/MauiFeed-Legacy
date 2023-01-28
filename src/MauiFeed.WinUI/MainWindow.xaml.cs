// <copyright file="MainWindow.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Modal;
using Drastic.Services;
using Drastic.Tools;
using MauiFeed.Models;
using MauiFeed.Services;
using MauiFeed.Translations;
using MauiFeed.Views;
using MauiFeed.WinUI.Pages;
using MauiFeed.WinUI.Services;
using MauiFeed.WinUI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Newtonsoft.Json.Linq;
using WinUIEx;

namespace MauiFeed.WinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WindowEx
    {
        private FeedTimelineSplitPage feedSplitPage;
        private SettingsPage settingsPage;
        private DatabaseContext context;
        private ThemeSelectorService themeSelectorService;
        private NavigationViewItemSeparator folderSeparator;
        private NavigationViewItemSeparator filterSeparator;
        private NavigationViewItem? addFolderButton;
        private Flyout? folderFlyout;
        private IErrorHandlerService errorHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.Activated += this.MainWindowActivated;
            this.context = Ioc.Default.GetService<DatabaseContext>()!;
            this.themeSelectorService = Ioc.Default.GetService<ThemeSelectorService>()!;
            this.errorHandler = Ioc.Default.GetService<IErrorHandlerService>()!;

            this.settingsPage = new SettingsPage();
            this.feedSplitPage = new FeedTimelineSplitPage(this);
            this.NavigationFrame.Content = this.feedSplitPage;

            this.ExtendsContentIntoAppTitleBar(true);
            this.SetTitleBar(this.AppTitleBar);

            this.GetAppWindow().TitleBar.ButtonBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
            this.GetAppWindow().TitleBar.ButtonInactiveBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);

            var manager = WinUIEx.WindowManager.Get(this);
            manager.Backdrop = new WinUIEx.MicaSystemBackdrop();

            this.folderSeparator = new NavigationViewItemSeparator();
            this.filterSeparator = new NavigationViewItemSeparator();
            this.RemoveFeedCommand = new AsyncCommand<FeedSidebarItem>(this.RemoveFeed, null, this.errorHandler);
            this.GenerateMenuButtons();
            this.GenerateSidebarItems();
        }

        /// <summary>
        /// Gets the list of sidebar items.
        /// </summary>
        public List<FeedSidebarItem> SidebarItems { get; } = new List<FeedSidebarItem>();

        /// <summary>
        /// Gets the remove feed command.
        /// </summary>
        public AsyncCommand<FeedSidebarItem> RemoveFeedCommand { get; }

        /// <summary>
        /// Gets the list of navigation items.
        /// </summary>
        public ObservableCollection<NavigationViewItemBase> Items { get; } = new ObservableCollection<NavigationViewItemBase>();

        /// <summary>
        /// Update sidebar items.
        /// </summary>
        public void UpdateSidebar()
        {
            foreach (var item in this.SidebarItems)
            {
                item.Update();
            }
        }

        /// <summary>
        /// Add Item To Sidebar.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void AddItemToSidebar(FeedListItem item)
        {
            var oldItem = this.SidebarItems.FirstOrDefault(n => n.FeedListItem?.Id == item.Id);

            // If we don't have this item already in the list, add it.
            // Then, update the sidebar. If you tried adding an existing item,
            // It would have updated the feed, so we can show the new items.
            if (oldItem is null)
            {
                var sidebarItem = new FeedSidebarItem(item!, this.context.FeedItems!.Include(n => n.Feed).Where(n => n.FeedListItemId == item.Id));
                sidebarItem.RightTapped += this.NavItemRightTapped;
                this.Items.Add(sidebarItem.NavItem);

                // BUG: What if you try and add it in twice?
                this.SidebarItems.Add(sidebarItem);
            }

            this.UpdateSidebar();
        }

        /// <summary>
        /// Remove a folder.
        /// </summary>
        /// <param name="item">Feed Folder.</param>
        /// <returns>Task.</returns>
        public Task RemoveFolder(FeedSidebarItem item)
        {
            this.addFolderButton?.ContextFlyout?.Hide();
            this.folderFlyout?.Hide();
            var feedFolder = item.FeedFolder!;
            if (this.feedSplitPage.SelectedSidebarItem == item)
            {
                var firstItem = this.SidebarItems.FirstOrDefault();
                this.NavView.SelectedItem = firstItem?.NavItem;
                this.feedSplitPage.SelectedItem = null;
            }

            this.context.FeedFolder!.Remove(feedFolder);
            this.context.SaveChangesAsync().FireAndForgetSafeAsync();
            this.SidebarItems.Remove(item);
            this.Items.Remove(item.NavItem);

            if (this.SidebarItems.Count(n => n.ItemType == SidebarItemType.Folder) <= 0)
            {
                this.Items.Remove(this.folderSeparator);
            }

            item.OnFolderDropped -= this.SidebarItemOnFolderDropped;
            item.RightTapped -= this.NavItemRightTapped;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Add a new folder.
        /// </summary>
        /// <param name="item">Feed Folder.</param>
        /// <returns>Task.</returns>
        public Task AddOrUpdateFolder(FeedSidebarItem item)
        {
            this.addFolderButton?.ContextFlyout?.Hide();
            this.folderFlyout?.Hide();

            var feedFolder = item.FeedFolder!;
            item.NavItem!.Content = feedFolder.Name;
            if (feedFolder.Id <= 0)
            {
                this.context.FeedFolder!.Add(feedFolder);
                item.OnFolderDropped += this.SidebarItemOnFolderDropped;
                item.NavItem.ContextFlyout = new Flyout() { Content = new FolderOptionsFlyout(this, item) };
                var folderIndex = this.Items.IndexOf(this.folderSeparator);
                if (folderIndex < 0)
                {
                    folderIndex = this.Items.IndexOf(this.filterSeparator);
                    this.Items.Insert(folderIndex + 1, item.NavItem);
                    this.Items.Insert(folderIndex + 2, this.folderSeparator);
                }
                else
                {
                    // Last one before the separator.
                    this.Items.Insert(folderIndex, item.NavItem);
                }

                this.SidebarItems.Add(item);
            }
            else
            {
                this.context.FeedFolder!.Update(feedFolder);
            }

            this.context.SaveChangesAsync().FireAndForgetSafeAsync();
            return Task.CompletedTask;
        }

        private void GenerateMenuButtons()
        {
            var refreshButton = new NavigationViewItem() { Content = Translations.Common.RefreshButton, Icon = new SymbolIcon(Symbol.Refresh) };
            refreshButton.SelectsOnInvoked = false;
            refreshButton.Tapped += (sender, args) =>
            {
                this.feedSplitPage.UpdateFeed();
            };

            this.Items.Add(refreshButton);

            var addButton = new NavigationViewItem() { Content = Translations.Common.AddLabel, Icon = new SymbolIcon(Symbol.Add) };
            addButton.SelectsOnInvoked = false;

            var addFeedButton = new NavigationViewItem() { Content = Translations.Common.FeedLabel, Icon = new SymbolIcon(Symbol.Library) };
            addFeedButton.SelectsOnInvoked = false;
            addFeedButton.ContextFlyout = new Flyout() { Content = new AddNewFeedFlyout(this) };
            addFeedButton.Tapped += this.MenuButtonTapped;

            addButton.MenuItems.Add(addFeedButton);

            this.addFolderButton = new NavigationViewItem() { Content = Translations.Common.FolderLabel, Icon = new SymbolIcon(Symbol.Folder) };
            this.addFolderButton.SelectsOnInvoked = false;
            this.addFolderButton.Tapped += this.AddFolderButtonTapped;
            addButton.MenuItems.Add(this.addFolderButton);

            this.Items.Add(addButton);
            this.Items.Add(new NavigationViewItemSeparator());
        }

        private void AddFolderButtonTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ((NavigationViewItem)sender).ContextFlyout = new Flyout() { Content = new FolderOptionsFlyout(this, new FeedSidebarItem(new FeedFolder())) };
            ((FrameworkElement)sender)!.ContextFlyout.ShowAt((FrameworkElement)sender!);
        }

        private void MenuButtonTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ((FrameworkElement)sender)!.ContextFlyout.ShowAt((FrameworkElement)sender!);
        }

        private void GenerateSidebarItems()
        {
            this.SidebarItems.Clear();
            this.GenerateSmartFilters();
            this.GenerateFolderItems();
            this.GenerateNavigationItems();
        }

        private void GenerateSmartFilters()
        {
            var smartFilters = new NavigationViewItem() { Content = Translations.Common.SmartFeedsLabel, Icon = new SymbolIcon(Symbol.Filter) };
            smartFilters.SelectsOnInvoked = false;

            var allButtonItem = new FeedSidebarItem(Translations.Common.AllLabel, new SymbolIcon(Symbol.Bookmarks), this.context.FeedItems!.Include(n => n.Feed));
            smartFilters.MenuItems.Add(allButtonItem.NavItem);
            this.SidebarItems.Add(allButtonItem);

            var today = new FeedSidebarItem(Translations.Common.TodayLabel, new SymbolIcon(Symbol.GoToToday), this.context.FeedItems!.Include(n => n.Feed).Where(n => n.PublishingDate != null && n.PublishingDate!.Value.Date == DateTime.UtcNow.Date));
            smartFilters.MenuItems.Add(today.NavItem);
            this.SidebarItems.Add(today);

            var unread = new FeedSidebarItem(Translations.Common.AllUnreadLabel, new SymbolIcon(Symbol.Filter), this.context.FeedItems!.Include(n => n.Feed).Where(n => !n.IsRead));
            smartFilters.MenuItems.Add(unread.NavItem);
            this.SidebarItems.Add(unread);

            var star = new FeedSidebarItem(Translations.Common.StarredLabel, new SymbolIcon(Symbol.Favorite), this.context.FeedItems!.Include(n => n.Feed).Where(n => n.IsFavorite));
            smartFilters.MenuItems.Add(star.NavItem);
            this.SidebarItems.Add(star);

            this.Items.Add(smartFilters);
            this.Items.Add(this.filterSeparator);
        }

        private void GenerateNavigationItems()
        {
            foreach (var item in this.context.FeedListItems!.Include(n => n.Items).Where(n => n.FolderId == null || n.FolderId <= 0))
            {
                var sidebarItem = new FeedSidebarItem(item!, this.context.FeedItems!.Include(n => n.Feed).Where(n => n.FeedListItemId == item.Id));
                sidebarItem.RightTapped += this.NavItemRightTapped;
                this.Items.Add(sidebarItem.NavItem);
                this.SidebarItems.Add(sidebarItem);
            }
        }

        private void GenerateFolderItems()
        {
            foreach (var item in this.context.FeedFolder!.Include(n => n.Items)!)
            {
                var (folder, feedSidebarItems) = this.GenerateFeedFolderSidebarItem(item);
                this.SidebarItems.Add(folder);
                this.SidebarItems.AddRange(feedSidebarItems);
                this.Items.Add(folder.NavItem);
            }

            // If we have folders, add the separator.
            if (this.SidebarItems.Any(n => n.ItemType == SidebarItemType.Folder))
            {
                this.Items.Add(this.folderSeparator);
            }
        }

        private (FeedSidebarItem Folder, List<FeedSidebarItem> FeedItems) GenerateFeedFolderSidebarItem(FeedFolder item)
        {
            var feedSidebarItems = new List<FeedSidebarItem>();
            var folder = new FeedSidebarItem(item, this.context.FeedItems!.Include(n => n.Feed).Where(n => (n.Feed!.FolderId ?? 0) == item.Id));
            folder.RightTapped += this.NavItemRightTapped;
            folder.OnFolderDropped += this.SidebarItemOnFolderDropped;
            foreach (var feed in item.Items!)
            {
                var sidebarItem = new FeedSidebarItem(feed, this.context.FeedItems!.Include(n => n.Feed).Where(n => n.FeedListItemId == feed.Id));
                sidebarItem.RightTapped += this.NavItemRightTapped;
                sidebarItem.NavItem.SetValue(Canvas.ZIndexProperty, 99);
                folder.NavItem.MenuItems.Add(sidebarItem.NavItem);
                feedSidebarItems.Add(sidebarItem);
            }

            return (folder, feedSidebarItems);
        }

        private void SidebarItemOnFolderDropped(object? sender, Events.FeedFolderDropEventArgs e)
        {
            var navItem = this.SidebarItems.FirstOrDefault(n => n.Id == e.FeedSidebarItemId);
            if (navItem is null)
            {
                return;
            }

            this.MoveItemToFolderAsync(navItem, e.Folder).FireAndForgetSafeAsync();
        }

        private async Task MoveItemToFolderAsync(FeedSidebarItem navigationViewItem, FeedSidebarItem folderViewItem)
        {
            if (folderViewItem.ItemType is not SidebarItemType.Folder)
            {
                return;
            }

            var feedListItem = navigationViewItem.FeedListItem!;
            var folder = folderViewItem.FeedFolder!;

            if (feedListItem.FolderId > 0)
            {
                var oldFolder = this.SidebarItems.FirstOrDefault(n => n.FeedFolder?.Id == feedListItem.FolderId);
                if (oldFolder is null)
                {
                    System.Diagnostics.Debug.Assert(oldFolder is not null, "Why does the folder not exist???");
                }
                else
                {
                    // Remove the existing item from the folder.
                    System.Diagnostics.Debug.Assert(oldFolder.NavItem.MenuItems.Remove(navigationViewItem.NavItem), "Was this removed???");
                }
            }

            await this.RemoveFromFolderAsync(navigationViewItem);

            feedListItem.Folder = folder;
            feedListItem.FolderId = folder.Id;

            // Update the database.
            this.context.FeedListItems!.Update(feedListItem);
            await this.context.SaveChangesAsync();

            // Generate new folder.
            var (navItem, feedSidebarItems) = this.GenerateFeedFolderSidebarItem(folder);

            foreach (var feedItem in feedSidebarItems)
            {
                this.SidebarItems[this.SidebarItems.IndexOf(this.SidebarItems.First(n => n.FeedListItem?.Id == feedItem.FeedListItem?.Id))] = feedItem;
            }

            this.Items[this.Items.IndexOf(folderViewItem.NavItem)] = navItem.NavItem;
            this.SidebarItems[this.SidebarItems.IndexOf(folderViewItem)] = navItem;

            this.UpdateSidebar();
        }

        private async Task RemoveFromFolderAsync(FeedSidebarItem navigationViewItem, bool moveToRoot = false)
        {
            if (navigationViewItem.FeedListItem?.FolderId > 0)
            {
                var oldFolder = this.SidebarItems.FirstOrDefault(n => n.ItemType is SidebarItemType.Folder && n.FeedFolder?.Id == navigationViewItem.FeedListItem.FolderId);
                if (oldFolder != null)
                {
                    oldFolder.NavItem.MenuItems.Remove(navigationViewItem);
                }
            }

            if (moveToRoot)
            {
                // Reset to null.
                this.Items.Add(navigationViewItem.NavItem);
                navigationViewItem.FeedListItem!.FolderId = null;
                navigationViewItem.FeedListItem!.Folder = null;
                this.context.FeedListItems!.Update(navigationViewItem.FeedListItem);
                await this.context.SaveChangesAsync();
            }
        }

        private void NavViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem nav)
            {
                var value = nav.Tag?.ToString() ?? string.Empty;
                switch (value)
                {
                    case "Settings":
                        this.NavigationFrame.Content = this.settingsPage;
                        break;
                    default:
                        this.feedSplitPage.SelectedSidebarItem = this.SidebarItems.FirstOrDefault(n => n.Id.ToString() == value);
                        System.Diagnostics.Debug.Assert(this.feedSplitPage.SelectedSidebarItem is not null, "Why is this null?");
                        this.NavigationFrame.Content = this.feedSplitPage;
                        break;
                }

                return;
            }
        }

        private void FeedSearchBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
        }

        private void MainWindowActivated(object sender, WindowActivatedEventArgs args)
        {
            this.Activated -= this.MainWindowActivated;
            this.themeSelectorService.SetRequestedTheme();
        }

        private void NavItemRightTapped(object? sender, Events.NavItemRightTappedEventArgs e)
        {
            var sidebarItem = e.SidebarItem;

            switch (sidebarItem.ItemType)
            {
                case SidebarItemType.SmartFilter:
                    break;
                case SidebarItemType.Folder:
                    this.folderFlyout = new Flyout() { Content = new FolderOptionsFlyout(this, sidebarItem) };
                    this.folderFlyout.ShowAt(sidebarItem.NavItem);
                    break;
                case SidebarItemType.FeedListItem:
                    var menuFlyout = new MenuFlyout();
                    var folders = this.SidebarItems.Where(n => n.ItemType == SidebarItemType.Folder).ToList();
                    var folderItemText = sidebarItem.FeedListItem?.FolderId > 0 ? Common.MoveToFolderLabel : Common.AddToFolderLabel;
                    var folderId = sidebarItem.FeedListItem?.FolderId ?? 0;
                    menuFlyout.Items.Add(new MenuFlyoutItem() { Icon = new SymbolIcon(Symbol.Globe), Text = Common.RemoveFeedLabel, Command = this.RemoveFeedCommand, CommandParameter = sidebarItem });
                    var folderItem = new MenuFlyoutSubItem() { Text = folderItemText, Icon = new SymbolIcon(Symbol.Folder) };
                    foreach (var item in folders.Where(n => n.FeedFolder?.Id != folderId))
                    {
                        var folder = new FolderMenuFlyoutItem(item, e.SidebarItem);
                        folder.Click += this.FolderClick;
                        folderItem.Items.Add(folder);
                    }

                    menuFlyout.Items.Add(folderItem);
                    menuFlyout.ShowAt(sidebarItem.NavItem);
                    break;
                default:
                    break;
            }
        }

        private void FolderClick(object sender, RoutedEventArgs e)
        {
            var folderMenu = (FolderMenuFlyoutItem)sender;
            this.MoveItemToFolderAsync(folderMenu.FeedItem, folderMenu.Folder).FireAndForgetSafeAsync();
        }

        private Task RemoveFeed(FeedSidebarItem item)
        {
            // If you're removing the item you have selected, reset the feed to All.
            if ((NavigationViewItem)this.NavView.SelectedItem == item.NavItem)
            {
                var firstItem = this.SidebarItems.FirstOrDefault();
                this.NavView.SelectedItem = firstItem?.NavItem;
                this.feedSplitPage.SelectedItem = null;
            }

            if (item.FeedListItem?.FolderId > 0)
            {
                var folder = this.SidebarItems.First(n => n.FeedFolder?.Id == item.FeedListItem.FolderId);
                folder.NavItem.MenuItems.Remove(item.NavItem);
            }
            else
            {
                this.Items.Remove(item.NavItem);
            }

            this.SidebarItems.Remove(item);
            this.context.FeedListItems!.Remove(item.FeedListItem!);

            this.UpdateSidebar();
            this.feedSplitPage.UpdateFeed();
            this.context.SaveChangesAsync().FireAndForgetSafeAsync();
            return Task.CompletedTask;
        }

        private class FolderMenuFlyoutItem : MenuFlyoutItem
        {
            public FolderMenuFlyoutItem(FeedSidebarItem folder, FeedSidebarItem feedItem)
            {
                this.Folder = folder;
                this.Text = folder.FeedFolder!.Name;
                this.FeedItem = feedItem;
            }

            /// <summary>
            /// Gets the Folder.
            /// </summary>
            public FeedSidebarItem Folder { get; }

            /// <summary>
            /// Gets the feed item.
            /// </summary>
            public FeedSidebarItem FeedItem { get; }
        }
    }
}
