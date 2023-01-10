// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using AngleSharp.Dom;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Services;
using Drastic.Tools;
using MauiFeed.Models;
using MauiFeed.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MauiFeed.WinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        private FeedNavigationViewItem? selectedNavItem;
        private EFCoreDatabaseContext databaseContext;
        private IErrorHandlerService errorHandler;
        private IAppDispatcher dispatcher;
        private ITemplateService templateService;

        public MainWindow()
        {
            this.databaseContext = Ioc.Default.ResolveWith<EFCoreDatabaseContext>();
            this.errorHandler = Ioc.Default.GetService<IErrorHandlerService>()!;
            this.dispatcher = Ioc.Default.GetService<IAppDispatcher>()!;
            this.templateService = Ioc.Default.GetService<ITemplateService>()!;

            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(this.AppTitleBar);
            var manager = WinUIEx.WindowManager.Get(this);
            manager.Backdrop = new WinUIEx.MicaSystemBackdrop();

            this.GenerateSmartFeeds();
            this.GenerateNavItems().FireAndForgetSafeAsync();

            this.MarkAsReadCommand = new AsyncCommand<FeedItem>(MarkAsRead, (x) => true, this.errorHandler);
            this.MarkAsFavoriteCommand = new AsyncCommand<FeedItem>(MarkAsFavorite, (x) => true, this.errorHandler);
            this.OpenInBrowserCommand = new AsyncCommand<FeedItem>(OpenInBrowser, (x) => true, this.errorHandler);
            this.ArticleList.DataContext = this;
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        public FeedNavigationViewItem? SelectedNavigationViewItem
        {
            get { return this.selectedNavItem; }
            set { this.SetProperty(ref this.selectedNavItem, value); }
        }

        public async Task MarkAsRead(FeedItem item)
        {
            item.IsRead = !item.IsRead;
            this.databaseContext.UpdateFeedItem(item).FireAndForgetSafeAsync();

        }

        public async Task OpenInBrowser(FeedItem item)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(item.Link!));
        }

        public async Task MarkAsFavorite(FeedItem item)
        {
            item.IsFavorite = !item.IsFavorite;
            this.databaseContext.UpdateFeedItem(item).FireAndForgetSafeAsync();
        }

        public AsyncCommand<FeedItem> MarkAsReadCommand { get; private set; }

        public AsyncCommand<FeedItem> MarkAsFavoriteCommand { get; private set; }

        public AsyncCommand<FeedItem> OpenInBrowserCommand { get; private set; }

        private FeedNavigationViewItem? smartFilters;

        public FeedNavigationViewItem? SelectedItem { get; set; }

        public void GenerateSmartFeeds()
        {
            this.smartFilters = new FeedNavigationViewItem(this.databaseContext);
            this.smartFilters.Content = Translations.Common.SmartFeedsLabel;
            this.smartFilters.Icon = new SymbolIcon(Symbol.Filter);

            var all = new FeedNavigationViewItem(this.databaseContext, this.databaseContext.CreateFilter<FeedItem, int>(o => o.Id, 0, EFCoreDatabaseContext.FilterType.GreaterThan));
            all.Content = Translations.Common.AllLabel;
            all.Icon = new SymbolIcon(Symbol.Bookmarks);
            this.smartFilters.MenuItems.Add(all);

            var today = new FeedNavigationViewItem(this.databaseContext, this.databaseContext.CreateFilter<FeedItem, DateTime?>(o => o.PublishingDate, DateTime.UtcNow.Date, EFCoreDatabaseContext.FilterType.GreaterThanOrEqual));
            today.Content = Translations.Common.TodayLabel;
            today.Icon = new SymbolIcon(Symbol.GoToToday);

            this.smartFilters.MenuItems.Add(today);

            var unread = new FeedNavigationViewItem(this.databaseContext, this.databaseContext.CreateFilter<FeedItem, bool>(o => o.IsRead, false, EFCoreDatabaseContext.FilterType.Equals));
            unread.Content = Translations.Common.AllUnreadLabel;
            unread.Icon = new SymbolIcon(Symbol.Filter);

            this.smartFilters.MenuItems.Add(unread);

            var star = new FeedNavigationViewItem(this.databaseContext, this.databaseContext.CreateFilter<FeedItem, bool>(o => o.IsFavorite, true, EFCoreDatabaseContext.FilterType.Equals));
            star.Content = Translations.Common.StarredLabel;
            star.Icon = new SymbolIcon(Symbol.Favorite);

            this.smartFilters.MenuItems.Add(star);

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
            var feedItems = await this.databaseContext.FeedListItems!.ToListAsync();
            foreach (var feed in feedItems)
            {
                this.Items.Add(this.GenerateNavItem(feed));
            }
        }

        private FeedNavigationViewItem GenerateNavItem(FeedListItem? item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.ImageCache is not byte[] cache)
            {
                throw new InvalidOperationException("ImageCache must not be null");
            }

            var icon = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
            icon.SetSource(cache.ToRandomAccessStream());

            var test = this.databaseContext.CreateFilter<FeedItem, int>(o => o.FeedListItemId, item.Id, EFCoreDatabaseContext.FilterType.Equals);

            return new FeedNavigationViewItem(this.databaseContext, test)
            {
                Tag = item.Id,
                Content = item.Name,
                Icon = new ImageIcon() { Source = icon, Width = 30, Height = 30, },
            };
        }

        public ObservableCollection<FeedNavigationViewItem> Items { get; set; } = new ObservableCollection<FeedNavigationViewItem>();

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
                this.dispatcher.Dispatch(this.UpdateMenuItems);
            }).FireAndForgetSafeAsync();


            Task.Run(async () => {
                var result = await this.templateService.RenderFeedItemAsync(selected.Feed!, selected);
                this.LocalRssWebview.SetSource(result);
            }).FireAndForgetSafeAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedNavigationViewItem is null)
            {
                return;
            }

            var items = this.SelectedNavigationViewItem.Items;
            foreach (var feedItem in items)
            {
                feedItem.IsRead = true;
            }

            Task.Run(async () =>
            {

                await this.databaseContext.UpdateFeedItems(items);
                this.dispatcher.Dispatch(this.UpdateMenuItems);

            }).FireAndForgetSafeAsync();
        }

        private void FeedSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            this.SelectedNavigationViewItem = new SearchFeedNavigationViewItem(args.QueryText, this.databaseContext) { Content = args.QueryText };
            var test = this.SelectedNavigationViewItem.Items;
        }

        private void UpdateMenuItems()
        {
            foreach (var item in this.smartFilters!.MenuItems.Cast<FeedNavigationViewItem>())
            {
                item?.Update();
            }

            this.SelectedItem?.Update();
        }
    }
}

public class SearchFeedNavigationViewItem : FeedNavigationViewItem
{
    private string searchTerm;

    public SearchFeedNavigationViewItem(string searchTerm, EFCoreDatabaseContext context, Expression<Func<FeedItem, bool>>? filter = null)
        : base(context, filter)
    {
        this.searchTerm = searchTerm;
    }

    public override IList<FeedItem> Items
    {
        get
        {
            return this.context.FeedItems!.Where(n => (n.Content ?? string.Empty).Contains(this.searchTerm)).OrderByDescending(n => n.PublishingDate).ToList();
        }
    }
}

public class FeedNavigationViewItem : NavigationViewItem, INotifyPropertyChanged
{
    internal EFCoreDatabaseContext context;

    public FeedNavigationViewItem(EFCoreDatabaseContext context, Expression<Func<FeedItem, bool>>? filter = default)
    {
        this.context = context;
        this.Filter = filter;
        this.Update();
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    public Expression<Func<FeedItem, bool>>? Filter { get; set; }

    public int ItemsCount => this.Items.Count;

    public int UnreadCount => this.Items.Where(n => !n.IsRead).Count();

    public virtual IList<FeedItem> Items
    {
        get
        {
            if (this.Filter is not null)
            {
                return this.context.FeedItems!.Where(this.Filter).OrderByDescending(n => n.PublishingDate).ToList();
            }

            return new List<FeedItem>();
        }
    }

    public void Update()
    {
        var count = this.UnreadCount;
        if (count > 0)
        {
            this.InfoBadge = new InfoBadge() { Value = count };
        }
        else
        {
            this.InfoBadge = null;
        }
    }

    /// <summary>
    /// On Property Changed.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        var changed = this.PropertyChanged;
        if (changed == null)
        {
            return;
        }

        changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// WinUI Extensions.
/// </summary>
public static class WinUIExtensions
{
    /// <summary>
    /// Create a random access stream from a byte array.
    /// </summary>
    /// <param name="array">The byte array.</param>
    /// <returns><see cref="IRandomAccessStream"/>.</returns>
    public static IRandomAccessStream ToRandomAccessStream(this byte[] array)
    {
        InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream();
        using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
        {
            writer.WriteBytes(array);
            writer.StoreAsync().GetResults();
        }

        return ms;
    }
}
