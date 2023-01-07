// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Tools;
using MauiFeed.Models;
using MauiFeed.Services;
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
        }

        public void GenerateSmartFeeds()
        {
            var smartFilters = new NavigationViewItem();
            smartFilters.Content = Translations.Common.SmartFeedsLabel;
            smartFilters.Icon = new SymbolIcon(Symbol.Filter);

            var all = new NavigationViewItem();
            all.Content = Translations.Common.AllLabel;
            all.Icon = new SymbolIcon(Symbol.Bookmarks);

            this.Items.Add(all);

            var today = new NavigationViewItem();
            today.Content = Translations.Common.TodayLabel;
            today.Icon = new SymbolIcon(Symbol.GoToToday);

            this.Items.Add(today);

            today.InfoBadge = new InfoBadge() { Value = 25 };

            var unread = new NavigationViewItem();
            unread.Content = Translations.Common.AllUnreadLabel;
            unread.Icon = new SymbolIcon(Symbol.Filter);

            this.Items.Add(unread);

            var star = new NavigationViewItem();
            star.Content = Translations.Common.StarredLabel;
            star.Icon = new SymbolIcon(Symbol.Favorite);

            this.Items.Add(star);
        }

        private NavigationViewItem GenerateNavItem(FeedListItem? item)
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

            return new NavigationViewItem() { Tag = item.Id, Content = item.Name, Icon = new ImageIcon() { Source = icon } };
        }

        public ObservableCollection<NavigationViewItem> Items { get; set; } = new ObservableCollection<NavigationViewItem>();
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
