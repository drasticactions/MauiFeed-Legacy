// <copyright file="TimelineCollectionViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using Drastic.PureLayout;
using MauiFeed.Models;

namespace MauiFeed.Apple
{
    public class TimelineCollectionViewController : UIViewController, IUICollectionViewDelegate, Views.ITimelineView
    {
        private IList<FeedItem> items;

        private UICollectionView collectionView;
        private UICollectionViewDiffableDataSource<NSString, MacFeedItem> dataSource;
        private RootSplitViewController controller;

        public TimelineCollectionViewController(RootSplitViewController controller)
        {
            this.controller = controller;
            this.items = new List<FeedItem>();
            this.collectionView = new UICollectionView(this.View!.Bounds, this.CreateLayout());

            var rowRegistration = UICollectionViewCellRegistration.GetRegistration(typeof(UICollectionViewListCell),
                new UICollectionViewCellRegistrationConfigurationHandler((cell, indexpath, item) =>
                    {
                        var listCell = (UICollectionViewListCell)cell;
                        var sidebarItem = (MacFeedItem)item;
                        var contentConfiguration = UIListContentConfiguration.CellConfiguration;
                        cell.ContentConfiguration = contentConfiguration;
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
            var sidebarItem = this.dataSource?.GetItemIdentifier(indexPath);
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