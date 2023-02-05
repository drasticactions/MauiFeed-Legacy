// <copyright file="FeedSidebarItem.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Runtime.CompilerServices;
using MauiFeed.Models;
using MauiFeed.Views;
using MauiFeed.WinUI.Events;
using MauiFeed.WinUI.Tools;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using WinUICommunity.Common.Extensions;

namespace MauiFeed.WinUI.Views
{
    /// <summary>
    /// Feed Sidebar Item.
    /// </summary>
    public class FeedSidebarItem : ISidebarItem, INotifyPropertyChanged
    {
        private Guid id;
        private bool hideUnreadItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSidebarItem"/> class.
        /// </summary>
        /// <param name="feedListItem">Feed List Item.</param>
        /// <param name="query">Optional query parameter.</param>
        public FeedSidebarItem(FeedListItem feedListItem, IQueryable<Models.FeedItem>? query = default)
        {
            this.id = Guid.NewGuid();
            this.FeedListItem = feedListItem;

            if (this.FeedListItem.ImageCache is not byte[] cache)
            {
                throw new InvalidOperationException("ImageCache must not be null");
            }

            var icon = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
            icon.SetSource(cache.ToRandomAccessStream());

            this.Query = query;
            this.NavItem = new NavigationViewItem() { Content = feedListItem.Name, Icon = new ImageIcon() { Source = icon, Width = 30, Height = 30, }, Tag = this.id };
            this.NavItem.CanDrag = true;
            this.NavItem.Loaded += this.NavItemLoaded;
            this.ItemType = SidebarItemType.FeedListItem;
            this.Update();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSidebarItem"/> class.
        /// </summary>
        /// <param name="folder">Feed Folder.</param>
        /// <param name="query">Optional query parameter.</param>
        public FeedSidebarItem(FeedFolder folder, IQueryable<FeedItem>? query = default)
        {
            this.id = Guid.NewGuid();
            this.Query = query;
            this.FeedFolder = folder;
            this.ItemType = SidebarItemType.Folder;
            this.NavItem = new NavigationViewItem() { Content = folder.Name, Icon = new SymbolIcon(Symbol.Folder), Tag = this.id };
            this.NavItem.Loaded += this.NavItemLoaded;
            this.NavItem.AllowDrop = true;
            this.Update();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSidebarItem"/> class.
        /// </summary>
        /// <param name="title">The Title.</param>
        /// <param name="icon">The Icon.</param>
        /// <param name="query">Optional query parameter.</param>
        public FeedSidebarItem(string title, IconElement icon, IQueryable<FeedItem>? query = default)
        {
            this.id = Guid.NewGuid();
            this.Query = query;
            this.NavItem = new NavigationViewItem() { Content = title, Icon = icon, Tag = this.id };
            this.ItemType = SidebarItemType.SmartFilter;
            this.Update();
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Event fired when folder gets item dropped.
        /// </summary>
        public event EventHandler<FeedFolderDropEventArgs>? OnFolderDropped;

        /// <summary>
        /// Event fired when nav item is right tapped.
        /// </summary>
        public event EventHandler<NavItemRightTappedEventArgs>? RightTapped;

        /// <summary>
        /// Gets the Id of the Feed Sidebar Item.
        /// </summary>
        public Guid Id => this.id;

        /// <summary>
        /// Gets the unread count.
        /// </summary>
        public int UnreadCount => this.Query?.Where(n => !n.IsRead).Count() ?? 0;

        /// <inheritdoc/>
        public FeedListItem? FeedListItem { get; }

        /// <inheritdoc/>
        public FeedFolder? FeedFolder { get; }

        /// <inheritdoc/>
        public IQueryable<FeedItem>? Query { get; }

        /// <inheritdoc/>
        public IList<FeedItem> Items
        {
            get
            {
                var query = this.Query;

                if (query is not null && this.HideUnreadItems)
                {
                    query = query.Where(n => !n.IsRead);
                }

                return query?.OrderByDescending(n => n.PublishingDate).ToList() ?? new List<FeedItem>();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to hide unread items.
        /// </summary>
        public bool AlwaysHideUnread { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide items.
        /// </summary>
        public bool HideUnreadItems
        {
            get { return this.hideUnreadItems; }
            set { this.SetProperty(ref this.hideUnreadItems, value); }
        }

        /// <summary>
        /// Gets a value indicating whether there are unread items in a given list.
        /// </summary>
        public bool HasUnreadItems => this.Items.Any(n => !n.IsRead);

        /// <summary>
        /// Gets the navigation view item.
        /// </summary>
        public NavigationViewItem NavItem { get; }

        /// <inheritdoc/>
        public string Title
        {
            get
            {
                if (this.NavItem.Content is string result)
                {
                    return result;
                }

                if (this.FeedListItem is not null)
                {
                    return this.FeedListItem.Name ?? string.Empty;
                }

                if (this.FeedFolder is not null)
                {
                    return this.FeedFolder.Name ?? string.Empty;
                }

                return string.Empty;
            }
        }

        /// <inheritdoc/>
        public SidebarItemType ItemType { get; }

        /// <inheritdoc/>
        public void Update()
        {
            var count = this.UnreadCount;
            if (count > 0)
            {
                this.NavItem.InfoBadge = new InfoBadge() { Value = count };
            }
            else
            {
                this.NavItem.InfoBadge = null;
            }
        }

        /// <summary>
        /// On Property Changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = this.PropertyChanged;
            if (changed == null)
            {
                return;
            }

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

#pragma warning disable SA1600 // Elements should be documented
        private bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null)
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

        private void NavItemLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var nav = (NavigationViewItem)sender!;
            nav.Loaded -= this.NavItemLoaded;
            var presenter = nav.FindDescendant<NavigationViewItemPresenter>();
            if (presenter is not null)
            {
                presenter.CanDrag = nav.CanDrag;
                presenter.AllowDrop = nav.AllowDrop;
                presenter.RightTapped += this.PresenterRightTapped;
                if (presenter.CanDrag)
                {
                    presenter.DragStarting += this.PresenterDragStarting;
                }

                if (presenter.AllowDrop)
                {
                    presenter.DragOver += this.PresenterDragOver;
                    presenter.Drop += this.PresenterDrop;
                }
            }
        }

        private void PresenterRightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            this.RightTapped?.Invoke(this, new NavItemRightTappedEventArgs(this));
        }

        private async void PresenterDrop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
        {
            var feedIdObject = await e.DataView.GetDataAsync(nameof(this.Id));
            if (feedIdObject is not Guid feedId)
            {
                return;
            }

            this.OnFolderDropped?.Invoke(this, new FeedFolderDropEventArgs(feedId, this));
        }

        private void PresenterDragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
        }

        private void PresenterDragStarting(Microsoft.UI.Xaml.UIElement sender, Microsoft.UI.Xaml.DragStartingEventArgs args)
        {
            args.Data.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
            args.Data.SetData(nameof(this.Id), this?.Id);
        }
    }
}
