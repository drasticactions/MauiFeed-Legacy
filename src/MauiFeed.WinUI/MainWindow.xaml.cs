// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.DependencyInjection;
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
    public sealed partial class MainWindow : Window
    {
        private EFCoreDatabaseContext databaseContext;

        public MainWindow()
        {
            this.databaseContext = Ioc.Default.ResolveWith<EFCoreDatabaseContext>();
            this.databaseContext.OnFeedItemUpdated += this.DatabaseContext_OnFeedItemUpdated;
            this.databaseContext.OnFeedListItemUpdated += this.DatabaseContext_OnFeedListItemUpdated;
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(this.AppTitleBar);
            var manager = WinUIEx.WindowManager.Get(this);
            manager.Backdrop = new WinUIEx.MicaSystemBackdrop();

            this.GenerateSmartFeeds();
            this.GenerateNavItems().FireAndForgetSafeAsync();
        }

        private void DatabaseContext_OnFeedListItemUpdated(object? sender, Events.FeedListItemUpdatedEventArgs e)
        {
            foreach (var item in this.Items)
            {
                item.Update();
                foreach (var childItem in item.MenuItems)
                {

                    ((FeedNavigationViewItem)childItem).Update();
                }
            }
        }

        private void DatabaseContext_OnFeedItemUpdated(object? sender, Events.FeedItemUpdatedEventArgs e)
        {
            foreach (var item in this.Items)
            {
                item.Update();
                foreach (var childItem in item.MenuItems)
                {

                    ((FeedNavigationViewItem)childItem).Update();
                }
            }
        }

        public void GenerateSmartFeeds()
        {
            var smartFilters = new FeedNavigationViewItem(this.databaseContext);
            smartFilters.Content = Translations.Common.SmartFeedsLabel;
            smartFilters.Icon = new SymbolIcon(Symbol.Filter);

            var all = new FeedNavigationViewItem(this.databaseContext, this.databaseContext.CreateFilter<FeedItem, int>(o => o.Id, 0, EFCoreDatabaseContext.FilterType.GreaterThan));
            all.Content = Translations.Common.AllLabel;
            all.Icon = new SymbolIcon(Symbol.Bookmarks);

            smartFilters.MenuItems.Add(all);

            var today = new FeedNavigationViewItem(this.databaseContext, this.databaseContext.CreateFilter<FeedItem, DateTime?>(o => o.PublishingDate, DateTime.UtcNow.Date, EFCoreDatabaseContext.FilterType.GreaterThanOrEqual));
            today.Content = Translations.Common.TodayLabel;
            today.Icon = new SymbolIcon(Symbol.GoToToday);

            smartFilters.MenuItems.Add(today);

            var unread = new FeedNavigationViewItem(this.databaseContext, this.databaseContext.CreateFilter<FeedItem, bool>(o => o.IsRead, false, EFCoreDatabaseContext.FilterType.Equals));
            unread.Content = Translations.Common.AllUnreadLabel;
            unread.Icon = new SymbolIcon(Symbol.Filter);

            smartFilters.MenuItems.Add(unread);

            var star = new FeedNavigationViewItem(this.databaseContext, this.databaseContext.CreateFilter<FeedItem, bool>(o => o.IsFavorite, true, EFCoreDatabaseContext.FilterType.Equals));
            star.Content = Translations.Common.StarredLabel;
            star.Icon = new SymbolIcon(Symbol.Favorite);

            smartFilters.MenuItems.Add(star);

            this.Items.Add(smartFilters);
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
                Icon = new ImageIcon() { Source = icon, Width = 30, Height = 30 },
            };
        }

        public ObservableCollection<FeedNavigationViewItem> Items { get; set; } = new ObservableCollection<FeedNavigationViewItem>();

        public ObservableCollection<FeedItem> FeedItems { get; set; } = new ObservableCollection<FeedItem>();

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is not FeedNavigationViewItem item)
            {
                return;
            }

            this.FeedItems.Clear();

            foreach (var feedItem in item.Items)
            {
                this.FeedItems.Add(feedItem);
            }
        }

        private void ArticleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.AddedItems.FirstOrDefault() as FeedItem;
            if (selected is null)
            {
                return;
            }

            selected.IsRead = true;
            this.databaseContext.UpdateFeedItem(selected).FireAndForgetSafeAsync();
        }
    }
}

public class FeedNavigationViewItem : NavigationViewItem, INotifyPropertyChanged
{
    private EFCoreDatabaseContext context;

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

    public IList<FeedItem> Items
    {
        get
        {
            if (this.Filter is not null)
            {
                return this.context.FeedItems!.Where(this.Filter).ToList();
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
