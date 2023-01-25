// <copyright file="TimelineCollectionViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.PureLayout;
using Drastic.Tools;
using MauiFeed.Models;
using MauiFeed.Services;
using MauiFeed.Views;
using ObjCRuntime;

namespace MauiFeed.Apple
{
    public class TimelineCollectionViewController : UIViewController, IUICollectionViewDelegate, Views.ITimelineView
    {
        private UICollectionView collectionView;
        private UICollectionViewDiffableDataSource<NSString, MacFeedItem> dataSource;
        private RootSplitViewController controller;
        private ISidebarItem? sidebarItem;
        private DatabaseContext database;

        public TimelineCollectionViewController(RootSplitViewController controller)
        {
            this.database = (DatabaseContext)Ioc.Default.GetService<DatabaseContext>()!;
            this.collectionView = new UICollectionView(this.View!.Bounds, this.CreateLayout());
            this.controller = controller;

            var rowRegistration = UICollectionViewCellRegistration.GetRegistration(
                typeof(FeedListCell),
                new UICollectionViewCellRegistrationConfigurationHandler((cell, indexpath, item) =>
                {
                    var listCell = (FeedListCell)cell;
                    var sidebarItem = (MacFeedItem)item;
                    listCell.SetupCell(sidebarItem.Item);
                }));

            this.dataSource = new UICollectionViewDiffableDataSource<NSString, MacFeedItem>(
                this.collectionView!,
                new UICollectionViewDiffableDataSourceCellProvider((collectionView, indexPath, item) =>
                {
                    var sidebarItem = (MacFeedItem)item;
                    return collectionView.DequeueConfiguredReusableCell(rowRegistration, indexPath, item);
                }));

            this.collectionView.Delegate = this;
#if !TVOS
            this.collectionView.BackgroundColor = UIColor.SystemBackground;
#endif
            this.View.AddSubview(this.collectionView);
            this.collectionView.TranslatesAutoresizingMaskIntoConstraints = false;
            this.collectionView.AutoPinEdgesToSuperviewEdges();
        }

        public ISidebarItem? SidebarItem
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

        public Task MarkAllAsRead(List<FeedItem> items)
        {
            return Task.CompletedTask;
        }

        public void SetFeed(ISidebarItem sidebar)
        {
            this.SidebarItem = sidebar;
        }

        public void UpdateFeed()
        {
            this.dataSource!.ApplySnapshot(this.ConfigureLocalSnapshot(), new NSString(string.Empty), false);
        }

        [Export("collectionView:didSelectItemAtIndexPath:")]
        protected void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var sidebarItem = this.dataSource.GetItemIdentifier(indexPath)!;
            sidebarItem!.Item.IsRead = true;

            this.database.UpdateFeedItem(sidebarItem!.Item).FireAndForgetSafeAsync();
            this.controller.FeedWebViewController.SetFeedItem(sidebarItem!.Item);
#if IOS
            this.controller.ShowColumn(UISplitViewControllerColumn.Secondary);
#endif
            this.controller.SidebarViewController.UpdateSidebar();
        }

        private NSDiffableDataSourceSectionSnapshot<MacFeedItem> ConfigureLocalSnapshot()
        {
            var snapshot = new NSDiffableDataSourceSectionSnapshot<MacFeedItem>();
            var items = new List<MacFeedItem>();

            foreach (var item in this.sidebarItem?.Items ?? new List<FeedItem>())
            {
                items.Add(new MacFeedItem(item));
            }

            snapshot.AppendItems(items.ToArray());
            return snapshot;
        }

        private UICollectionViewLayout CreateLayout()
        {
            return new UICollectionViewCompositionalLayout((sectionIndex, layoutEnvironment) =>
            {
                var configuration = new UICollectionLayoutListConfiguration(UICollectionLayoutListAppearance.Plain);
                configuration.ShowsSeparators = true;
                configuration.HeaderMode = UICollectionLayoutListHeaderMode.None;
                return NSCollectionLayoutSection.GetSection(configuration, layoutEnvironment);
            });
        }

        private class FeedListCell : UICollectionViewListCell
        {
            private FeedItem? item;
            private UILabel contentLabel = new UILabel();
            private UIView hasSeenHolder = new UIView();
            private UIImageView hasSeenIcon = new UIImageView();

            protected internal FeedListCell(NativeHandle handle)
                : base(handle)
            {
                this.ContentView.AddSubview(this.hasSeenHolder);
                this.hasSeenHolder.AddSubview(this.hasSeenIcon);

                this.hasSeenHolder.AutoPinEdgesToSuperviewEdgesExcludingEdge(UIEdgeInsets.Zero, ALEdge.Right);
                this.hasSeenHolder.AutoSetDimension(ALDimension.Width, 25f);

                this.hasSeenIcon.AutoCenterInSuperview();
                this.hasSeenIcon.AutoSetDimensionsToSize(new CGSize(12, 12));
            }

            public void SetupCell(FeedItem item)
            {
                if (this.item is not null)
                {
                    this.item.PropertyChanged -= this.Item_PropertyChanged;
                }

                this.item = item;
                this.item.PropertyChanged += this.Item_PropertyChanged;

                // this.contentLabel.Text = item.Title;
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

            private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                this.UpdateIsRead();
            }
        }

        private class MacFeedItem : NSObject
        {
            public MacFeedItem(FeedItem item)
            {
                this.Item = item;
                this.Image = UIImage.LoadFromData(NSData.FromArray(item.Feed!.ImageCache!));
            }

            public UIImage? Image { get; }

            public FeedItem Item { get; }
        }
    }
}