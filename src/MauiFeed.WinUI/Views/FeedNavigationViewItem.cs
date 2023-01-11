﻿// <copyright file="FeedNavigationViewItem.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Linq.Expressions;
using MauiFeed.Models;
using MauiFeed.Services;
using MauiFeed.Views;
using MauiFeed.WinUI.Tools;
using Microsoft.UI.Xaml.Controls;

namespace MauiFeed.WinUI.Views
{
    public class FeedNavigationViewItem : NavigationViewItem, ISidebarItem
    {
        internal DatabaseContext context;

        public FeedNavigationViewItem(string title, IconElement icon, DatabaseContext context, Expression<Func<FeedItem, bool>>? filter = default)
        {
            this.Content = title;
            this.Icon = icon;
            this.context = context;
            this.Filter = filter;
            this.Update();
        }

        public FeedNavigationViewItem(string title, FeedListItem item, DatabaseContext context, Expression<Func<FeedItem, bool>>? filter = default)
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

            this.context = context;
            this.Filter = filter;
            this.Update();
        }

        public Expression<Func<FeedItem, bool>>? Filter { get; set; }

        public int ItemsCount => this.Items.Count;

        public int UnreadCount => this.Items.Where(n => !n.IsRead).Count();

        public FeedListItem? FeedListItem { get; }

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
    }
}