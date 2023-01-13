// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Services;
using Drastic.Tools;
using MauiFeed.Models;
using MauiFeed.Services;
using MauiFeed.Views;
using MauiFeed.WinUI.Services;
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MauiFeed.WinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FeedTimelineSplitView : Page, ITimelineView, INotifyPropertyChanged
    {
        private ISidebarItem? selectedNavItem;
        private ISidebarView sidebar;
        private IErrorHandlerService errorHandler;
        private IAppDispatcher dispatcher;
        private ITemplateService templateService;
        private ThemeSelectorService themeSelectorService;

        private DatabaseContext databaseContext;

        public FeedTimelineSplitView(ISidebarView window, ThemeSelectorService themeSelectorService)
        {
            this.InitializeComponent();

            this.databaseContext = Ioc.Default.ResolveWith<DatabaseContext>();
            this.errorHandler = Ioc.Default.GetService<IErrorHandlerService>()!;
            this.dispatcher = Ioc.Default.GetService<IAppDispatcher>()!;
            this.templateService = Ioc.Default.GetService<ITemplateService>()!;
            this.sidebar = window;
            this.MarkAsReadCommand = new AsyncCommand<FeedItem>(this.MarkAsRead, (x) => true, this.errorHandler);
            this.MarkAsFavoriteCommand = new AsyncCommand<FeedItem>(this.MarkAsFavorite, (x) => true, this.errorHandler);
            this.OpenInBrowserCommand = new AsyncCommand<FeedItem>(this.OpenInBrowser, (x) => true, this.errorHandler);
            this.MarkAllAsReadCommand = new AsyncCommand<FeedNavigationViewItem>((x) => this.MarkAllAsRead(x.Items.ToList()), (x) => true, this.errorHandler);

            this.ArticleList.DataContext = this;
            this.themeSelectorService = themeSelectorService;
            this.themeSelectorService.ThemeChanged += ThemeSelectorService_ThemeChanged;
        }

        private void ThemeSelectorService_ThemeChanged(object? sender, EventArgs e)
        {
            this.RenderFeedItem(this.SelectedItem);
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<FeedItem> Items { get; private set; } = new ObservableCollection<FeedItem>();

        public FeedItem? SelectedItem { get; set; }

        public AsyncCommand<FeedItem> MarkAsReadCommand { get; private set; }

        public AsyncCommand<FeedNavigationViewItem> MarkAllAsReadCommand { get; private set; }

        public AsyncCommand<FeedItem> MarkAsFavoriteCommand { get; private set; }

        public AsyncCommand<FeedItem> OpenInBrowserCommand { get; private set; }

        public ISidebarItem? SelectedNavigationViewItem
        {
            get { return this.selectedNavItem; }
            set { this.SetProperty(ref this.selectedNavItem, value); }
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
            this.sidebar.UpdateSidebar();
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

        public void SetFeed(ISidebarItem sidebar)
        {
            this.SelectedNavigationViewItem = sidebar;
            this.UpdateFeed();
        }

        private void ArticleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.AddedItems.FirstOrDefault() as FeedItem;
            if (selected is null)
            {
                return;
            }

            this.SelectedItem = selected;

            selected.IsRead = true;

            Task.Run(async () =>
            {
                await this.databaseContext.UpdateFeedItem(selected);
                this.dispatcher.Dispatch(this.sidebar.UpdateSidebar);
            }).FireAndForgetSafeAsync();

            this.RenderFeedItem(selected);
        }

        private void RenderFeedItem(FeedItem? item)
        {
            if (item is null)
            {
                return;
            }

            Task.Run(async () =>
            {
                var result = await this.templateService.RenderFeedItemAsync(item, this.themeSelectorService.IsDark);
                this.LocalRssWebview.SetSource(result);
            }).FireAndForgetSafeAsync();
        }

        public void UpdateFeed()
        {
            this.Items.Clear();

            var items = this.selectedNavItem?.Items ?? new List<FeedItem>();

            foreach (var item in items)
            {
                this.Items.Add(item);
            }
        }
    }
}
