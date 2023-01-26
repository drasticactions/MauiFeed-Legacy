// <copyright file="MainWindow.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Modal;
using MauiFeed.Models;
using MauiFeed.Services;
using MauiFeed.Views;
using MauiFeed.WinUI.Pages;
using MauiFeed.WinUI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            this.context = Ioc.Default.GetService<DatabaseContext>()!;

            this.settingsPage = new SettingsPage();
            this.feedSplitPage = new FeedTimelineSplitPage();
            this.NavigationFrame.Content = this.feedSplitPage;

            this.ExtendsContentIntoAppTitleBar(true);
            this.SetTitleBar(this.AppTitleBar);

            this.GetAppWindow().TitleBar.ButtonBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
            this.GetAppWindow().TitleBar.ButtonInactiveBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);

            var manager = WinUIEx.WindowManager.Get(this);
            manager.Backdrop = new WinUIEx.MicaSystemBackdrop();

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

            addButton.MenuItems.Add(addFeedButton);

            var addFolderButton = new NavigationViewItem() { Content = Translations.Common.FolderLabel, Icon = new SymbolIcon(Symbol.Folder) };
            addFolderButton.SelectsOnInvoked = false;

            addButton.MenuItems.Add(addFolderButton);

            this.Items.Add(addButton);
            this.Items.Add(new NavigationViewItemSeparator());
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

            var allButtonItem = new FeedSidebarItem(Translations.Common.AllLabel, new SymbolIcon(Symbol.Bookmarks), this.context.FeedItems!.Include(n => n.Feed).Where(n => n.Id > 0));
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
            this.Items.Add(new NavigationViewItemSeparator());
        }

        private void GenerateNavigationItems()
        {
            foreach (var item in this.context.FeedListItems!.Where(n => n.FolderId <= 0))
            {
                var sidebarItem = new FeedSidebarItem(item!);
                this.Items.Add(sidebarItem.NavItem);
                this.SidebarItems.Add(sidebarItem);
            }
        }

        private void GenerateFolderItems()
        {
            foreach (var item in this.context.FeedFolder!.Include(n => n.Items)!)
            {
                var folder = new FeedSidebarItem(item, this.context.FeedItems!.Include(n => n.Feed).Where(n => (n.Feed!.FolderId ?? 0) == item.Id).OrderByDescending(n => n.PublishingDate));

                foreach (var feed in item.Items!)
                {
                    var sidebarItem = new FeedSidebarItem(item!);
                    folder.NavItem.MenuItems.Add(sidebarItem.NavItem);
                    this.SidebarItems.Add(sidebarItem);
                }

                this.Items.Add(folder.NavItem);
                this.SidebarItems.Add(folder);
            }
        }

        private void NavViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem nav)
            {
                switch (nav.Tag?.ToString() ?? string.Empty)
                {
                    case "Settings":
                        this.NavigationFrame.Content = this.settingsPage;
                        break;
                    default:
                        this.NavigationFrame.Content = this.feedSplitPage;
                        break;
                }

                return;
            }
        }

        private void FeedSearchBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
        }
    }
}
