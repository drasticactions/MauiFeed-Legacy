// <copyright file="TimelineCollectionViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.PureLayout;
using MauiFeed.Models;
using MauiFeed.Services;
using ObjCRuntime;
using Drastic.Tools;

namespace MauiFeed.Apple
{
    public class TimelineCollectionViewController : UIViewController, IUICollectionViewDelegate, Views.ITimelineView
    {
        private IList<FeedItem> items;

        private UICollectionView collectionView;
        private UICollectionViewDiffableDataSource<NSString, MacFeedItem> dataSource;
        private RootSplitViewController controller;
        private EFCoreDatabaseContext database;

        public TimelineCollectionViewController(RootSplitViewController controller)
        {
            this.database = (EFCoreDatabaseContext)Ioc.Default.GetService<IDatabaseService>()!;
            this.controller = controller;
            this.items = new List<FeedItem>();
            this.collectionView = new UICollectionView(this.View!.Bounds, this.CreateLayout());

            var rowRegistration = UICollectionViewCellRegistration.GetRegistration(typeof(FeedListCell),
                new UICollectionViewCellRegistrationConfigurationHandler((cell, indexpath, item) =>
                    {
                        var listCell = (FeedListCell)cell;
                        var sidebarItem = (MacFeedItem)item;
                        listCell.SetupCell(sidebarItem.Item);
                }));

            this.dataSource = new UICollectionViewDiffableDataSource<NSString, MacFeedItem>(collectionView!,
                new UICollectionViewDiffableDataSourceCellProvider((collectionView, indexPath, item) =>
                {
                    var sidebarItem = (MacFeedItem)item;
                    return collectionView.DequeueConfiguredReusableCell(rowRegistration, indexPath, item);
                })
            );

            this.collectionView.Delegate = this;
#if !TVOS
            this.collectionView.BackgroundColor = UIColor.SystemBackground;
#endif
            this.View.AddSubview(this.collectionView);
            this.collectionView.TranslatesAutoresizingMaskIntoConstraints = false;
            this.collectionView.AutoPinEdgesToSuperviewEdges();
        }

        public void SetFeedItems(IList<FeedItem> feedItems)
        {
            this.items = feedItems;
            this.dataSource!.ApplySnapshot(this.ConfigureLocalSnapshot(), new NSString(string.Empty), false);
        }

        [Export("collectionView:didSelectItemAtIndexPath:")]
        protected void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var sidebarItem = this.dataSource.GetItemIdentifier(indexPath)!;
            sidebarItem!.Item.IsRead = true;

            this.database.AddOrUpdateFeedItem(sidebarItem!.Item).FireAndForgetSafeAsync();
            this.controller.FeedWebViewController.SetFeedItem(sidebarItem!.Item);
#if IOS
            this.controller.ShowColumn(UISplitViewControllerColumn.Secondary);
#endif
        }

        private NSDiffableDataSourceSectionSnapshot<MacFeedItem> ConfigureLocalSnapshot()
        {
            var snapshot = new NSDiffableDataSourceSectionSnapshot<MacFeedItem>();
            var items = new List<MacFeedItem>();

            foreach (var item in this.items ?? new List<FeedItem>())
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
#if !TVOS
                configuration.ShowsSeparators = true;
#endif
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

                //this.contentLabel.Text = item.Title;

                this.UpdateIsRead();
            }

            private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                this.UpdateIsRead();
            }

            public void UpdateIsRead()
            {
                if (this.item?.IsFavorite ?? false)
                {
                    this.hasSeenIcon.Image = UIImage.GetSystemImage("circle.fill")!.ApplyTintColor(UIColor.Yellow);
                }
                else
                {
                    this.hasSeenIcon.Image = this.item?.IsRead ?? false ? UIImage.GetSystemImage("circle") : UIImage.GetSystemImage("circle.fill");
                }
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