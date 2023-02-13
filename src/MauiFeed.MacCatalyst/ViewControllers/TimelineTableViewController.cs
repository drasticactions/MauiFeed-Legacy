// <copyright file="TimelineTableViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.PureLayout;
using Drastic.Services;
using Drastic.Tools;
using MauiFeed.MacCatalyst.Sidebar;
using MauiFeed.MacCatalyst.Tools;
using MauiFeed.Models;
using MauiFeed.Services;

namespace MauiFeed.MacCatalyst.ViewControllers
{
    /// <summary>
    /// Timeline Table View Controller.
    /// </summary>
    public class TimelineTableViewController : UIViewController
    {
        private SidebarItem? sidebarItem;
        private RootSplitViewController controller;
        private DatabaseContext database;
        private FeedItem? selectedItem;
        private IErrorHandlerService errorHandler;
        private RssTableView tableView;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineTableViewController"/> class.
        /// </summary>
        /// <param name="controller">Root View Controller.</param>
        public TimelineTableViewController(RootSplitViewController controller)
        {
            this.controller = controller;
            this.database = (DatabaseContext)Ioc.Default.GetService<DatabaseContext>()!;
            this.errorHandler = (IErrorHandlerService)Ioc.Default.GetService<IErrorHandlerService>()!;
            this.tableView = new RssTableView(this.View!.Frame, UITableViewStyle.Plain);
            this.View!.AddSubview(this.tableView);
            this.tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            this.tableView.AutoPinEdgesToSuperviewEdges();
        }

        /// <summary>
        /// Gets or sets the sidebar item.
        /// </summary>
        public SidebarItem? SidebarItem
        {
            get
            {
                return this.sidebarItem;
            }

            set
            {
                this.sidebarItem = value;
                this.UpdateFeed();
            }
        }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        public FeedItem? SelectedItem
        {
            get
            {
                return this.selectedItem;
            }

            set
            {
                this.selectedItem = value;
                this.UpdateSelectedFeedItemAsync().FireAndForgetSafeAsync(this.errorHandler);
            }
        }

        /// <summary>
        /// Gets a value indicating whether to show the icon.
        /// </summary>
        public bool ShowIcon
        {
            get
            {
                // If it's a smart filter or folder, always show the icon.
                if (this.sidebarItem?.ItemType != SidebarItemType.FeedListItem)
                {
                    return true;
                }

                var feed = this.sidebarItem?.Items.Select(n => n.Feed).Distinct() ?? new List<FeedListItem>();
                return feed.Count() > 1;
            }
        }

        /// <summary>
        /// Update the selected feed item, if it exists.
        /// </summary>
        /// <returns>Task.</returns>
        public async Task UpdateSelectedFeedItemAsync()
        {
            if (this.SelectedItem is null)
            {
                return;
            }

            this.database.FeedItems!.Update(this.SelectedItem);
            await this.database.SaveChangesAsync();

            this.controller.Sidebar.UpdateSidebar();
        }

        /// <summary>
        /// Update the given feed.
        /// </summary>
        public void UpdateFeed()
        {
            var items = this.sidebarItem?.Items ?? new List<FeedItem>();
            this.tableView.Source = new TableSource(this, items.ToArray());
            this.tableView.ReloadData();
        }

        private class RssTableView : UITableView
        {
            public RssTableView(CGRect rect, UITableViewStyle style)
                : base(rect, style)
            {
                this.RowHeight = 100f;
            }
        }

        private class TableSource : UITableViewSource
        {
            private FeedItem[] tableItems;
            private TimelineTableViewController controller;
            private bool showIcon;

            public TableSource(TimelineTableViewController controller, FeedItem[] items)
            {
                this.controller = controller;
                this.tableItems = items;
                this.showIcon = this.controller.ShowIcon;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return this.tableItems.Length;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                RssItemViewCell? cell = tableView.DequeueReusableCell(RssItemViewCell.PublicReuseIdentifier) as RssItemViewCell;
                FeedItem item = this.tableItems[indexPath.Row];

                if (cell == null)
                {
                    cell = new RssItemViewCell(item, this.showIcon);
                }
                else
                {
                    cell.SetupCell(item, this.showIcon);
                }

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                this.controller.SelectedItem = this.tableItems[indexPath.Row];
                this.controller.SelectedItem.IsRead = true;
                var cell = (RssItemViewCell)tableView.CellAt(indexPath)!;
                cell.UpdateIsRead();
            }
        }

        private class RssItemViewCell : UITableViewCell
        {
            private FeedItem item;

            private UIView hasSeenHolder = new UIView();
            private UIView iconHolder = new UIView();
            private UIView feedHolder = new UIView();

            private UIView content = new UIView();

            private UIImageView hasSeenIcon = new UIImageView();
            private UIImageView icon = new UIImageView();
            private UILabel title = new UILabel() { Lines = 3, Font = UIFont.PreferredHeadline, TextAlignment = UITextAlignment.Left };
            private UILabel description = new UILabel() { Lines = 2, Font = UIFont.PreferredSubheadline, TextAlignment = UITextAlignment.Left };
            private UILabel releaseDate = new UILabel() { Lines = 1, Font = UIFont.PreferredCaption1, TextAlignment = UITextAlignment.Right };
            private UILabel author = new UILabel() { Lines = 1, Font = UIFont.PreferredCaption1, TextAlignment = UITextAlignment.Left };

            public RssItemViewCell(FeedItem info, bool showIcon = false, UITableViewCellStyle style = UITableViewCellStyle.Default)
          : base(style, PublicReuseIdentifier)
            {
                this.item = info;
                this.icon.Layer.CornerRadius = 5;
                this.icon.Layer.MasksToBounds = true;
                this.SetupUI();
                this.SetupLayout();
                this.SetupCell(info, showIcon);
            }

            /// <summary>
            /// Gets the Reuse Identifier.
            /// </summary>
            public static NSString PublicReuseIdentifier => new NSString("rssItemCell");

            public void SetupUI()
            {
                this.ContentView.AddSubview(this.content);

                this.content.AddSubview(this.hasSeenHolder);
                this.content.AddSubview(this.iconHolder);
                this.content.AddSubview(this.feedHolder);

                this.hasSeenHolder.AddSubview(this.hasSeenIcon);

                this.iconHolder.AddSubview(this.icon);

                this.feedHolder.AddSubview(this.title);
                this.feedHolder.AddSubview(this.description);
                this.feedHolder.AddSubview(this.author);
                this.feedHolder.AddSubview(this.releaseDate);

                this.hasSeenIcon.Image = UIImage.GetSystemImage("circle.fill");
            }

            public void SetupLayout()
            {
                this.content.AutoPinEdgesToSuperviewEdges();
                this.content.AutoSetDimension(ALDimension.Height, 70f);

                this.hasSeenHolder.AutoPinEdgesToSuperviewEdgesExcludingEdge(UIEdgeInsets.Zero, ALEdge.Right);
                this.hasSeenHolder.AutoPinEdge(ALEdge.Right, ALEdge.Left, this.iconHolder);
                this.hasSeenHolder.AutoSetDimension(ALDimension.Width, 25f);

                this.iconHolder.AutoPinEdge(ALEdge.Left, ALEdge.Right, this.hasSeenHolder);
                this.iconHolder.AutoPinEdge(ALEdge.Right, ALEdge.Left, this.feedHolder);
                this.iconHolder.AutoPinEdge(ALEdge.Top, ALEdge.Top, this.content);
                this.iconHolder.AutoPinEdge(ALEdge.Bottom, ALEdge.Bottom, this.content);
                this.iconHolder.AutoSetDimension(ALDimension.Width, 60f);

                this.hasSeenIcon.AutoCenterInSuperview();
                this.hasSeenIcon.AutoSetDimensionsToSize(new CGSize(12, 12));

                this.icon.AutoCenterInSuperview();
                this.icon.AutoSetDimensionsToSize(new CGSize(50f, 50f));

                this.feedHolder.AutoPinEdgesToSuperviewEdgesExcludingEdge(new UIEdgeInsets(top: 0f, left: 0f, bottom: 0f, right: 0f), ALEdge.Left);
                this.feedHolder.AutoPinEdge(ALEdge.Left, ALEdge.Right, this.iconHolder);

                this.title.AutoPinEdge(ALEdge.Top, ALEdge.Top, this.feedHolder, 5f);
                this.title.AutoPinEdge(ALEdge.Right, ALEdge.Right, this.feedHolder, -15f);
                this.title.AutoPinEdge(ALEdge.Left, ALEdge.Left, this.feedHolder, 10f);

                this.description.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, this.title, 0);
                this.description.AutoPinEdge(ALEdge.Right, ALEdge.Right, this.title);
                this.description.AutoPinEdge(ALEdge.Left, ALEdge.Left, this.title);

                this.author.AutoPinEdge(ALEdge.Bottom, ALEdge.Bottom, this.content, -5);
                this.author.AutoPinEdge(ALEdge.Left, ALEdge.Left, this.title);
                this.author.AutoPinEdge(ALEdge.Right, ALEdge.Left, this.releaseDate);

                this.releaseDate.AutoPinEdge(ALEdge.Bottom, ALEdge.Bottom, this.content,　-5f);
                this.releaseDate.AutoPinEdge(ALEdge.Right, ALEdge.Right, this.title);
                this.releaseDate.AutoPinEdge(ALEdge.Left, ALEdge.Right, this.author);
            }

            public void SetupCell(FeedItem item, bool showIcon)
            {
                this.item = item;

                this.icon.Image = UIImage.LoadFromData(NSData.FromArray(item.Feed!.ImageCache!))!.Scale(new CGSize(50f, 50f), 2f).WithRoundedCorners(5f);
                this.title.Text = item.Title;
                this.author.Text = item.Author;

                var htmlString = !string.IsNullOrEmpty(item.Description) ? item.Description : item.Content;

                // We don't want to render the HTML, we just want to get the raw text out.
                this.description.Text = Regex.Replace(htmlString ?? string.Empty, "<[^>]*>", string.Empty)!.Trim();

                this.releaseDate.Text = item.PublishingDate?.ToShortDateString();

                this.UpdateIsRead();
            }

            public void UpdateIsRead()
            {
                if (this.item?.IsFavorite ?? false)
                {
                    this.InvokeOnMainThread(() => this.hasSeenIcon.Image = UIImage.GetSystemImage("circle.fill")!.ApplyTintColor(UIColor.Yellow));
                }
                else
                {
                    this.InvokeOnMainThread(() => this.hasSeenIcon.Image = this.item?.IsRead ?? false ? UIImage.GetSystemImage("circle") : UIImage.GetSystemImage("circle.fill"));
                }
            }
        }
    }
}