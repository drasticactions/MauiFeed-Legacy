// <copyright file="FeedCollectionViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using MauiFeed.Models;
using Drastic.PureLayout;
using MauiFeed.Services;

namespace MauiFeed.Apple
{
    public class FeedCollectionViewController : UIViewController, IUICollectionViewDelegate, Views.ITimelineView
    {
        private IList<FeedItem>? selectedItem;

        private UICollectionViewDiffableDataSource<NSString, MacFeedItem>? dataSource;
        private UICollectionView? collectionView;

        public FeedCollectionViewController()
        {
        }

        public void SetFeedItems(IList<FeedItem> items)
        {
            this.selectedItem = items;
            this.ApplyInitialSnapshot();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.ConfigureCollectionView();
            this.ConfigureDataSource();
            this.ApplyInitialSnapshot();
        }

        private void ApplyInitialSnapshot()
        {
            this.dataSource!.ApplySnapshot(this.ConfigureLocalSnapshot(), new NSString(string.Empty), false);
        }

        private NSDiffableDataSourceSectionSnapshot<MacFeedItem> ConfigureLocalSnapshot()
        {
            var snapshot = new NSDiffableDataSourceSectionSnapshot<MacFeedItem>();
            var items = new List<MacFeedItem>();

            foreach (var item in this.selectedItem ?? new List<FeedItem>())
            {
                items.Add(new MacFeedItem(item));
            }

            snapshot.AppendItems(items.ToArray());
            return snapshot;
        }

        private void ConfigureDataSource()
        {
            var rowRegistration = UICollectionViewCellRegistration.GetRegistration(typeof(UICollectionViewListCell),
                 new UICollectionViewCellRegistrationConfigurationHandler((cell, indexpath, item) =>
                 {
                     var listCell = (UICollectionViewListCell)cell;
                     var sidebarItem = (MacFeedItem)item;
                     var contentConfiguration = UIListContentConfiguration.CellConfiguration;
                     contentConfiguration.Text = sidebarItem.Item.Title;
                     contentConfiguration.TextProperties.NumberOfLines = 2;
                     contentConfiguration.SecondaryText = sidebarItem.Item.Content;
                     contentConfiguration.SecondaryTextProperties.NumberOfLines = 2;
                     contentConfiguration.Image = sidebarItem.Image!.Scale(new CGSize(42, 42), 2f);
                     contentConfiguration.TextProperties.Font = UIFont.PreferredSubheadline;
                     contentConfiguration.TextProperties.Color = UIColor.SecondaryLabel;
                     cell.ContentConfiguration = contentConfiguration;
                 }));

            this.dataSource = new UICollectionViewDiffableDataSource<NSString, MacFeedItem>(collectionView!,
                new UICollectionViewDiffableDataSourceCellProvider((collectionView, indexPath, item) =>
                {
                    var sidebarItem = (MacFeedItem)item;
                    return collectionView.DequeueConfiguredReusableCell(rowRegistration, indexPath, item);
                })
            );
        }

        private void ConfigureCollectionView()
        {
            this.collectionView = new UICollectionView(this.View!.Bounds, this.CreateLayout());
            this.collectionView.Delegate = this;
#if !TVOS
            this.collectionView.BackgroundColor = UIColor.SystemBackground;
#endif
            this.View.AddSubview(this.collectionView);
            this.collectionView.TranslatesAutoresizingMaskIntoConstraints = false;
            this.collectionView.AutoPinEdgesToSuperviewEdges();
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