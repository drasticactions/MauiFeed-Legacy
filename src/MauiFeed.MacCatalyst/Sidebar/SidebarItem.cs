// <copyright file="SidebarItem.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using MauiFeed.Models;
using MauiFeed.Services;

namespace MauiFeed.MacCatalyst.Sidebar
{
    /// <summary>
    /// Sidebar Item.
    /// </summary>
    public class SidebarItem : NSObject, MauiFeed.Views.ISidebarItem, INotifyPropertyChanged
    {
        private Guid id;
        private bool hideUnreadItems;
        private string? customTitle;

        /// <summary>
        /// Initializes a new instance of the <see cref="SidebarItem"/> class.
        /// </summary>
        /// <param name="headerTitle">Header Title.</param>
        /// <param name="rowType">Row Type.</param>
        /// <param name="itemType">Item Type.</param>
        public SidebarItem(string headerTitle, SidebarItemRowType rowType, SidebarItemType itemType)
        {
            this.customTitle = headerTitle;
            this.RowType = SidebarItemRowType.Header;
            this.ItemType = itemType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SidebarItem"/> class.
        /// </summary>
        /// <param name="feedListItem">Feed List Item.</param>
        /// <param name="query">Optional query parameter.</param>
        public SidebarItem(FeedListItem feedListItem, IQueryable<Models.FeedItem>? query = default)
        {
            this.id = Guid.NewGuid();
            this.Query = query;

            this.FeedListItem = feedListItem;
            if (this.FeedListItem.ImageCache is not null)
            {
                this.Image = UIImage.LoadFromData(NSData.FromArray(this.FeedListItem.ImageCache!))!.Scale(new CGSize(16, 16), 2f);
            }

            this.RowType = SidebarItemRowType.Row;
            this.ItemType = SidebarItemType.FeedListItem;
            this.Update();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SidebarItem"/> class.
        /// </summary>
        /// <param name="folder">Feed Folder.</param>
        /// <param name="query">Optional query parameter.</param>
        public SidebarItem(FeedFolder folder, IQueryable<FeedItem>? query = default)
        {
            this.id = Guid.NewGuid();
            this.Query = query;
            this.FeedFolder = folder;
            this.Image = UIImage.GetSystemImage("folder");
            this.RowType = SidebarItemRowType.ExpandableRow;
            this.ItemType = SidebarItemType.Folder;
            this.Update();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SidebarItem"/> class.
        /// </summary>
        /// <param name="title">The Title.</param>
        /// <param name="icon">The Icon.</param>
        /// <param name="query">Optional query parameter.</param>
        public SidebarItem(string title, UIImage icon, IQueryable<FeedItem>? query = default)
        {
            this.id = Guid.NewGuid();
            this.Query = query;
            this.customTitle = title;
            this.Image = icon;
            this.RowType = SidebarItemRowType.Row;
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
        /// Gets the image.
        /// </summary>
        public UIImage? Image { get; }

        /// <summary>
        /// Gets the unread count.
        /// </summary>
        public int UnreadCount => this.Query?.Where(n => !n.IsRead).Count() ?? 0;

        /// <summary>
        /// Gets or sets the given sidebar list cell.
        /// </summary>
        public SidebarListCell? Cell { get; set; }

        /// <inheritdoc/>
        public FeedListItem? FeedListItem { get; }

        /// <inheritdoc/>
        public FeedFolder? FeedFolder { get; }

        /// <inheritdoc/>
        public IQueryable<FeedItem>? Query { get; }

        /// <inheritdoc/>
        public SidebarItemType ItemType { get; }

        /// <summary>
        /// Gets the row type.
        /// </summary>
        public SidebarItemRowType RowType { get; }

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

        /// <inheritdoc/>
        public string Title
        {
            get
            {
                if (!string.IsNullOrEmpty(this.customTitle))
                {
                    return this.customTitle;
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

        /// <inheritdoc/>
        public void Update()
        {
            this.Cell?.UpdateIsRead();
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