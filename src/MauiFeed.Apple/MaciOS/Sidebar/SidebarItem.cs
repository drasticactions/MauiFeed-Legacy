using System;
using MauiFeed.Models;
using System.Linq.Expressions;
using MauiFeed.Services;
using Microsoft.EntityFrameworkCore;

namespace MauiFeed.Apple
{
    public class SidebarItem : NSObject
    {
        private DatabaseContext context;

        public Guid Id { get; }

        public SidebarItemType Type { get; }

        public string Title { get; }

        public UIImage? Image { get; }

        public SidebarSection Section { get; }

        public SidebarListCell? Cell { get; set; }

        public SidebarItem(Guid id, DatabaseContext context, SidebarItemType type, SidebarSection section, string title, UIImage? image = default, Expression<Func<FeedItem, bool>>? filter = default)
        {
            this.Id = id;
            this.Type = type;
            this.Section = section;
            this.Title = title;
            this.Image = image;
            this.Filter = filter;
            this.context = context;
        }

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

        public void Update()
        {
            this.Cell?.UpdateIsRead();
        }

        public static SidebarItem Header(DatabaseContext context, string title, SidebarSection section, Guid? id = default, Expression<Func<FeedItem, bool>>? filter = default)
            => new SidebarItem(id ?? Guid.NewGuid(), context, SidebarItemType.Header, section, title, filter: filter);

        public static SidebarItem ExpandableRow(DatabaseContext context, string title, SidebarSection section, UIImage? image = default, Guid? id = default, Expression<Func<FeedItem, bool>>? filter = default)
         => new SidebarItem(id ?? Guid.NewGuid(), context, SidebarItemType.ExpandableRow, section, title, image, filter);

        public static SidebarItem Row(DatabaseContext context, string title, SidebarSection section, FeedListItem? subtitle = default, UIImage? image = default, Guid? id = default, Expression<Func<FeedItem, bool>>? filter = default)
            => new SidebarItem(id ?? Guid.NewGuid(), context, SidebarItemType.Row, section, title, image, filter);
    }
}