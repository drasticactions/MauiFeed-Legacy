// <copyright file="MainWindow.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Modal;
using Drastic.Tools;
using MauiFeed.Models;
using MauiFeed.Services;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.Activated += this.MainWindowActivated;
            this.context = Ioc.Default.GetService<DatabaseContext>()!;
            this.themeSelectorService = Ioc.Default.GetService<ThemeSelectorService>()!;

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
            this.GenerateMenuButtons();
            this.GenerateSidebarItems();
        }

        /// <summary>
        /// Gets the list of sidebar items.
        /// </summary>
        public List<FeedSidebarItem> SidebarItems { get; } = new List<FeedSidebarItem>();

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
            item.NavItem.ContextFlyout?.Hide();
            var feedFolder = item.FeedFolder!;
            if (this.feedSplitPage.SelectedSidebarItem == item)
            {
                var firstItem = this.SidebarItems.FirstOrDefault();
                this.NavView.SelectedItem = firstItem?.NavItem;
            }

            this.context.FeedFolder!.Remove(feedFolder);
            this.context.SaveChangesAsync().FireAndForgetSafeAsync();
            this.SidebarItems.Remove(item);
            this.Items.Remove(item.NavItem);

            if (this.SidebarItems.Count(n => n.ItemType == SidebarItemType.Folder) <= 0)
            {
                this.Items.Remove(this.folderSeparator);
            }

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
            item.NavItem.ContextFlyout?.Hide();

            var feedFolder = item.FeedFolder!;
            item.NavItem!.Content = feedFolder.Name;
            if (feedFolder.Id <= 0)
            {
                this.context.FeedFolder!.Add(feedFolder);
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
            }
            else
            {
                this.context.FeedFolder!.Update(feedFolder);
            }

            this.SidebarItems.Add(item);
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
                this.Items.Add(sidebarItem.NavItem);
                this.SidebarItems.Add(sidebarItem);
            }
        }

        private void GenerateFolderItems()
        {
            foreach (var item in this.context.FeedFolder!.Include(n => n.Items)!)
            {
                var folder = new FeedSidebarItem(item, this.context.FeedItems!.Include(n => n.Feed).Where(n => (n.Feed!.FolderId ?? 0) == item.Id).OrderByDescending(n => n.PublishingDate));
                folder.NavItem.ContextFlyout = new Flyout() { Content = new FolderOptionsFlyout(this, folder) };
                foreach (var feed in item.Items!)
                {
                    var sidebarItem = new FeedSidebarItem(item!);
                    folder.NavItem.MenuItems.Add(sidebarItem.NavItem);
                    this.SidebarItems.Add(sidebarItem);
                }

                this.Items.Add(folder.NavItem);
                this.SidebarItems.Add(folder);
            }

            // If we have folders, add the separator.
            if (this.SidebarItems.Any(n => n.ItemType == SidebarItemType.Folder))
            {
                this.Items.Add(this.folderSeparator);
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
    }
}
