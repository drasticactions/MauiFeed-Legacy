// <copyright file="SidebarItem.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Linq.Expressions;
using MauiFeed.Models;
using MauiFeed.Services;
using Microsoft.EntityFrameworkCore;

namespace MauiFeed.Apple
{
    public class SidebarItem : NSObject, Views.ISidebarItem
    {
        private DatabaseContext context;

        public SidebarItem(Guid id, DatabaseContext context, SidebarItemRowType rowType, SidebarSection section, string title, SidebarItemType type, UIImage? image = default, Expression<Func<FeedItem, bool>>? filter = default, FeedListItem? item = default)
        {
            this.Id = id;
            this.Type = rowType;
            this.ItemType = type;
            this.Section = section;
            this.Title = title;
            this.Image = image;
            this.Filter = filter;
            this.context = context;
            this.FeedListItem = item;
        }

        public Guid Id { get; }

        public string Title { get; }

        public UIImage? Image { get; }

        public SidebarSection Section { get; }

        public SidebarItemRowType Type { get; }

        public SidebarListCell? Cell { get; set; }

        public int ItemsCount => this.Items.Count;

        public int UnreadCount => this.Items.Where(n => !n.IsRead).Count();

        public Expression<Func<FeedItem, bool>>? Filter { get; set; }

        public virtual IList<FeedItem> Items
        {
            get
            {
                if (this.Filter is not null)
                {
                    return this.context.FeedItems!.Include(n => n.Feed).Where(this.Filter).OrderByDescending(n => n.PublishingDate).ToList();
                }

                return new List<FeedItem>();
            }
        }

        public FeedListItem? FeedListItem { get; set; }

        public SidebarItemType ItemType { get; }

        public static SidebarItem Header(DatabaseContext context, string title, SidebarSection section, Guid? id = default, Expression<Func<FeedItem, bool>>? filter = default, SidebarItemType type = SidebarItemType.FeedListItem)
            => new SidebarItem(id ?? Guid.NewGuid(), context, SidebarItemRowType.Header, section, title, type, filter: filter);

        public void Update()
        {
            this.Cell?.UpdateIsRead();
        }

        public static SidebarItem ExpandableRow(DatabaseContext context, string title, SidebarSection section, UIImage? image = default, Guid? id = default, Expression<Func<FeedItem, bool>>? filter = default, SidebarItemType type = SidebarItemType.FeedListItem)
         => new SidebarItem(id ?? Guid.NewGuid(), context, SidebarItemRowType.ExpandableRow, section, title, type, image, filter);

        public static SidebarItem Row(DatabaseContext context, string title, SidebarSection section, FeedListItem? subtitle = default, UIImage? image = default, Guid? id = default, Expression<Func<FeedItem, bool>>? filter = default, SidebarItemType type = SidebarItemType.FeedListItem)
            => new SidebarItem(id ?? Guid.NewGuid(), context, SidebarItemRowType.Row, section, title, type, image, filter);
    }
}