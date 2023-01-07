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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(this.AppTitleBar);
            var manager = WinUIEx.WindowManager.Get(this);
            manager.Backdrop = new WinUIEx.MicaSystemBackdrop();

            this.GenerateSmartFeeds();
            this.GenerateNavItems().FireAndForgetSafeAsync();
        }

        public void GenerateSmartFeeds()
        {
            var smartFilters = new FeedNavigationViewItem(this.databaseContext);
            smartFilters.Content = Translations.Common.SmartFeedsLabel;
            smartFilters.Icon = new SymbolIcon(Symbol.Filter);

            var all = new FeedNavigationViewItem(this.databaseContext);
            all.Content = Translations.Common.AllLabel;
            all.Icon = new SymbolIcon(Symbol.Bookmarks);

            smartFilters.MenuItems.Add(all);

            var today = new FeedNavigationViewItem(this.databaseContext);
            today.Content = Translations.Common.TodayLabel;
            today.Icon = new SymbolIcon(Symbol.GoToToday);
            today.Filter = this.databaseContext.CreateFilter<FeedItem, DateTime>(o => o.PublishingDate!.Value, DateTime.UtcNow);

            smartFilters.MenuItems.Add(today);

            today.InfoBadge = new InfoBadge() { Value = 25 };

            var unread = new FeedNavigationViewItem(this.databaseContext);
            unread.Content = Translations.Common.AllUnreadLabel;
            unread.Icon = new SymbolIcon(Symbol.Filter);
            unread.Filter = this.databaseContext.CreateFilter<FeedItem, bool>(o => o.IsRead, false);

            smartFilters.MenuItems.Add(unread);

            var star = new FeedNavigationViewItem(this.databaseContext);
            star.Content = Translations.Common.StarredLabel;
            star.Icon = new SymbolIcon(Symbol.Favorite);
            star.Filter = this.databaseContext.CreateFilter<FeedItem, bool>(o => o.IsFavorite, true);

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

            var test = this.databaseContext.CreateFilter<FeedItem, int>(o => o.FeedListItemId, item.Id);

            return new FeedNavigationViewItem(this.databaseContext)
            {
                Tag = item.Id,
                Content = item.Name,
                Icon = new ImageIcon() { Source = icon, Width = 30, Height = 30 },
                Filter = test,
            };
        }

        private FeedNavigationViewItem GenerateFeedNavigationViewItem()
        {
            var icon = new FeedNavigationViewItem(this.databaseContext);

            return icon;
        }

        public ObservableCollection<FeedNavigationViewItem> Items { get; set; } = new ObservableCollection<FeedNavigationViewItem>();
    }
}

public class FeedNavigationViewItem : NavigationViewItem
{
    private EFCoreDatabaseContext context;

    public FeedNavigationViewItem(EFCoreDatabaseContext context)
    {
        this.context = context;
    }

    public Expression<Func<FeedItem, bool>>? Filter { get; set; }

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
