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

namespace MauiFeed.WinUI
{
    /// <summary>
    /// The main window of the app.
    /// </summary>
    public sealed partial class MainWindow : Window, ISidebarView, INotifyPropertyChanged
    {
        private FeedNavigationViewItem? selectedNavItem;
        private DatabaseContext databaseContext;
        private IErrorHandlerService errorHandler;
        private IAppDispatcher dispatcher;
        private ITemplateService templateService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.databaseContext = Ioc.Default.ResolveWith<DatabaseContext>();
            this.errorHandler = Ioc.Default.GetService<IErrorHandlerService>()!;
            this.dispatcher = Ioc.Default.GetService<IAppDispatcher>()!;
            this.templateService = Ioc.Default.GetService<ITemplateService>()!;

            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(this.AppTitleBar);
            var manager = WinUIEx.WindowManager.Get(this);
            manager.Backdrop = new WinUIEx.MicaSystemBackdrop();

            this.AddNewFeedCommand = new AsyncCommand<string>(AddNewFeed, (x) => true, this.errorHandler);
            this.RemoveFeedCommand = new AsyncCommand<FeedListItem>(RemoveFeed, (x) => true, this.errorHandler);
            this.MarkAsReadCommand = new AsyncCommand<FeedItem>(MarkAsRead, (x) => true, this.errorHandler);
            this.MarkAsFavoriteCommand = new AsyncCommand<FeedItem>(MarkAsFavorite, (x) => true, this.errorHandler);
            this.OpenInBrowserCommand = new AsyncCommand<FeedItem>(OpenInBrowser, (x) => true, this.errorHandler);
            this.MarkAllAsReadCommand = new AsyncCommand<FeedNavigationViewItem>((x) => this.MarkAllAsRead(x.Items.ToList()), (x) => true, this.errorHandler);

            this.GenerateSidebar();

            this.ArticleList.DataContext = this;
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        public FeedNavigationViewItem? SelectedNavigationViewItem
        {
            get { return this.selectedNavItem; }
            set { this.SetProperty(ref this.selectedNavItem, value); }
        }

        public ObservableCollection<FeedNavigationViewItem> Items { get; set; } = new ObservableCollection<FeedNavigationViewItem>();

        public AsyncCommand<string> AddNewFeedCommand { get; private set; }

        public AsyncCommand<FeedListItem> RemoveFeedCommand { get; private set; }

        public AsyncCommand<FeedItem> MarkAsReadCommand { get; private set; }

        public AsyncCommand<FeedNavigationViewItem> MarkAllAsReadCommand { get; private set; }

        public AsyncCommand<FeedItem> MarkAsFavoriteCommand { get; private set; }

        public AsyncCommand<FeedItem> OpenInBrowserCommand { get; private set; }

        /// <inheritdoc/>
        public void UpdateSidebar()
        {
            foreach (var item in this.Items)
            {
                item.Update();
            }
        }

        /// <inheritdoc/>
        public void GenerateSidebar()
        {
            this.GenerateSmartFeeds();
            this.GenerateNavItems().FireAndForgetSafeAsync(this.errorHandler);
        }

        public void GenerateSmartFeeds()
        {
            var smartFilters = new FeedNavigationViewItem(Translations.Common.SmartFeedsLabel, new SymbolIcon(Symbol.Filter), this.databaseContext);

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

        /// <summary>
        /// On Property Changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.dispatcher.Dispatch(() =>
            {
                var changed = this.PropertyChanged;
                if (changed == null)
                {
                    return;
                }

                changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

#pragma warning disable SA1600 // Elements should be documented
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null)
#pragma warning restore SA1600 // Elements should be documented
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                return false;
            }

            backingStore = value;
            onChanged?.Invoke();
            this.OnPropertyChanged(propertyName);
            return true;
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

        public async Task AddNewFeed(string feedUri)
        {
            Uri.TryCreate(feedUri, UriKind.Absolute, out Uri? uri);
            if (uri is null)
            {
                return;
            }

            this.UpdateSidebar();
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

        /// <summary>
        /// Mark the feed item as read or unread.
        /// </summary>
        /// <param name="item">The feed item to mark.</param>
        /// <returns>Task.</returns>
        public Task MarkAsRead(FeedItem item)
            => this.MarkAllAsRead(new List<FeedItem> { item });

        /// <summary>
        /// Mark the feed item as read or unread.
        /// </summary>
        /// <param name="item">The feed item to mark.</param>
        /// <returns>Task.</returns>
        public async Task MarkAllAsRead(List<FeedItem> items)
        {
            var allRead = items.All(n => n.IsRead);

            foreach (var item in items)
            {
                item.IsRead = !allRead;
            }

            this.databaseContext.UpdateFeedItems(items).FireAndForgetSafeAsync();
            this.UpdateSidebar();
        }

        /// <summary>
        /// Open the feed item in a browser.
        /// </summary>
        /// <param name="item">The Feed Item to open.</param>
        /// <returns>Task.</returns>
        public async Task OpenInBrowser(FeedItem item)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(item.Link!));
        }

        /// <summary>
        /// Mark the feed item as a favorite.
        /// </summary>
        /// <param name="item">Feed item to mark.</param>
        /// <returns>Task.</returns>
        public async Task MarkAsFavorite(FeedItem item)
        {
            item.IsFavorite = !item.IsFavorite;
            this.databaseContext.UpdateFeedItem(item).FireAndForgetSafeAsync();
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is not FeedNavigationViewItem item)
            {
                return;
            }

            this.SelectedNavigationViewItem = item;
        }

        private void ArticleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.AddedItems.FirstOrDefault() as FeedItem;
            if (selected is null)
            {
                return;
            }

            selected.IsRead = true;

            Task.Run(async () =>
            {
                await this.databaseContext.UpdateFeedItem(selected);
                this.dispatcher.Dispatch(this.UpdateSidebar);
            }).FireAndForgetSafeAsync();


            Task.Run(async () => {
                var result = await this.templateService.RenderFeedItemAsync(selected);
                this.LocalRssWebview.SetSource(result);
            }).FireAndForgetSafeAsync();
        }
    }
}
