// <copyright file="FeedNavigationViewItem.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using CommunityToolkit.WinUI.UI;
using Drastic.Tools;
using MauiFeed.Events;
using MauiFeed.Models;
using MauiFeed.Services;
using MauiFeed.Views;
using MauiFeed.WinUI.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace MauiFeed.WinUI.Views
{
    public class FeedNavigationViewItem : NavigationViewItem, ISidebarItem, INotifyPropertyChanged
    {
        private bool hideUnreadItems;

        internal DatabaseContext Context;

        public FeedNavigationViewItem(string title, FeedFolder folder, IconElement icon, DatabaseContext context, Expression<Func<FeedItem, bool>>? filter = default, SidebarItemType itemType = SidebarItemType.FeedListItem)
            : this(title, icon, context, filter, itemType)
        {
            this.Folder = folder;
        }

        public FeedNavigationViewItem(string title, IconElement icon, DatabaseContext context, Expression<Func<FeedItem, bool>>? filter = default, SidebarItemType itemType = SidebarItemType.FeedListItem)
        {
            this.Content = title;
            this.Icon = icon;
            this.Context = context;
            this.Filter = filter;
            this.Update();
            this.ItemType = itemType;
        }

        public FeedNavigationViewItem(string title, FeedListItem item, DatabaseContext context, Expression<Func<FeedItem, bool>>? filter = default, SidebarItemType type = SidebarItemType.FeedListItem)
        {
            if (item.ImageCache is not byte[] cache)
            {
                throw new InvalidOperationException("ImageCache must not be null");
            }

            this.FeedListItem = item;

            var icon = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
            icon.SetSource(cache.ToRandomAccessStream());
            this.Content = title;
            this.Icon = new ImageIcon() { Source = icon, Width = 30, Height = 30, };

            this.Context = context;
            this.Filter = filter;
            this.ItemType = type;
            this.Update();
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<FeedFolderDropEventArgs>? OnFolderDrop;

        public Expression<Func<FeedItem, bool>>? Filter { get; set; }

        public int ItemsCount => this.Items.Count;

        public int UnreadCount => this.Items.Where(n => !n.IsRead).Count();

        public FeedListItem? FeedListItem { get; }

        public FeedFolder? Folder { get; }

        public bool AlwaysHideUnread { get; set; }

        public bool HideUnreadItems
        {
            get { return this.hideUnreadItems; }
            set
            {
                this.SetProperty(ref this.hideUnreadItems, value);
            }
        }

        public string Title => this.Content as string ?? string.Empty;

        public virtual IList<FeedItem> Items
        {
            get
            {
                IQueryable<FeedItem>? query = null;

                if (this.ItemType == SidebarItemType.Folder)
                {
                    var folderId = this.Folder?.Id ?? 0;
                    query = this.Context.FeedItems!.Include(n => n.Feed).Where(n => (n.Feed!.FolderId ?? 0) == folderId).OrderByDescending(n => n.PublishingDate);
                }

                if (this.Filter is not null)
                {
                    query = this.Context.FeedItems!.Include(n => n.Feed).Where(this.Filter).OrderByDescending(n => n.PublishingDate);
                }

                if (query is not null && this.HideUnreadItems)
                {
                    query = query.Where(n => !n.IsRead);
                }

                return query?.ToList() ?? new List<FeedItem>();
            }
        }

        public SidebarItemType ItemType { get; }

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

        public void SetDragAndDrop(bool canDrag, bool allowDrop)
        {
            this.CanDrag = canDrag;
            this.AllowDrop = allowDrop;
            this.Loaded += this.FeedNavigationViewItem_Loaded;
        }

        public void Update()
        {
            foreach (var item in this.MenuItems.Cast<FeedNavigationViewItem>())
            {
                item.Update();
            }

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

        private void FeedNavigationViewItem_DragStarting(Microsoft.UI.Xaml.UIElement sender, Microsoft.UI.Xaml.DragStartingEventArgs args)
        {
            args.Data.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
            args.Data.SetData(nameof(this.FeedListItem), this.FeedListItem?.Id);
        }

        private void FeedNavigationViewItem_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var nav = (FeedNavigationViewItem)sender!;
            nav.Loaded -= this.FeedNavigationViewItem_Loaded;
            var presenter = nav.FindDescendant<NavigationViewItemPresenter>();
            if (presenter is not null)
            {
                presenter.CanDrag = this.CanDrag;
                presenter.AllowDrop = this.AllowDrop;
                if (presenter.CanDrag)
                {
                    presenter.DragStarting += this.FeedNavigationViewItem_DragStarting;
                }

                if (presenter.AllowDrop)
                {
                    presenter.DragOver += Presenter_DragOver;
                    presenter.Drop += this.FeedNavigationViewItem_Drop;
                }
            }
        }

        private void Presenter_DragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
        }

        private async void FeedNavigationViewItem_Drop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
        {
            var feedIdObject = await e.DataView.GetDataAsync(nameof(this.FeedListItem));
            if (feedIdObject is not int feedId)
            {
                return;
            }

            this.OnFolderDrop?.Invoke(this, new FeedFolderDropEventArgs(this, feedId));
        }
    }
}
