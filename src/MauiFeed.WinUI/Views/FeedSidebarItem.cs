// <copyright file="FeedSidebarItem.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Runtime.CompilerServices;
using MauiFeed.Models;
using MauiFeed.Views;
using MauiFeed.WinUI.Tools;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

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
        public FeedSidebarItem(FeedListItem feedListItem, IQueryable<FeedItem>? query = default)
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

                return query?.ToList() ?? new List<FeedItem>();
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
        /// Gets the navigation view item.
        /// </summary>
        public NavigationViewItem NavItem { get; }

        /// <inheritdoc/>
        public string Title => this.NavItem.Content as string ?? string.Empty;

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
    }
}
