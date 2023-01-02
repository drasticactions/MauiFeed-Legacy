#if !TVOS
// <copyright file="SidebarViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using Drastic.PureLayout;
using ObjCRuntime;

namespace MauiFeed.Apple
{
    public class SidebarViewController : UIViewController, IUICollectionViewDelegate
    {
        private Guid smartFilterRowIdentifier = Guid.NewGuid();
        private Guid localRowIdentifier = Guid.NewGuid();

        private UICollectionViewDiffableDataSource<NSString, SidebarItem>? dataSource;
        private UICollectionView? collectionView;

        public SidebarViewController()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.ConfigureCollectionView();
            this.ConfigureDataSource();
            this.ApplyInitialSnapshot();
        }

        [Export("collectionView:didSelectItemAtIndexPath:")]
        protected void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var sidebarItem = this.dataSource?.GetItemIdentifier(indexPath);
        }

        private void ConfigureCollectionView()
        {
            this.collectionView = new UICollectionView(this.View!.Bounds, this.CreateLayout());
            this.collectionView.Delegate = this;
            this.collectionView.BackgroundColor = UIColor.SystemBackground;
            this.View.AddSubview(this.collectionView);
            this.collectionView.TranslatesAutoresizingMaskIntoConstraints = false;
            this.collectionView.AutoPinEdgesToSuperviewEdges();
        }

        private NSDiffableDataSourceSectionSnapshot<SidebarItem> ConfigureSmartFeedSnapshot()
        {
            var snapshot = new NSDiffableDataSourceSectionSnapshot<SidebarItem>();
            var header = SidebarItem.Header("Smart Feeds");
            var items = new SidebarItem[]
            {
                SidebarItem.Row("Today", null, UIImage.GetSystemImage("sun.max")),
                SidebarItem.Row("All Unread", null, UIImage.GetSystemImage("circle.inset.filled")),
                SidebarItem.Row("Starred", null, UIImage.GetSystemImage("star.fill")),
            };

            snapshot.AppendItems(new[] { header });
            snapshot.ExpandItems(new[] { header });
            snapshot.AppendItems(items, header);
            return snapshot;
        }

        private NSDiffableDataSourceSectionSnapshot<SidebarItem> ConfigureLocalSnapshot()
        {
            var snapshot = new NSDiffableDataSourceSectionSnapshot<SidebarItem>();
            var header = SidebarItem.Header("Local");
            var items = new SidebarItem[]
            {
            };

            snapshot.AppendItems(new[] { header });
            snapshot.ExpandItems(new[] { header });
            snapshot.AppendItems(items, header);
            return snapshot;
        }

        private void ApplyInitialSnapshot()
        {
            this.dataSource!.ApplySnapshot(this.ConfigureSmartFeedSnapshot(), new NSString(SidebarSection.SmartFeeds.ToString()), false);
            this.dataSource!.ApplySnapshot(this.ConfigureLocalSnapshot(), new NSString(SidebarSection.Local.ToString()), false);
        }

        private UICollectionViewLayout CreateLayout()
        {
            return new UICollectionViewCompositionalLayout((sectionIndex, layoutEnvironment) =>
            {
                var configuration = new UICollectionLayoutListConfiguration(UICollectionLayoutListAppearance.Sidebar);
                configuration.ShowsSeparators = false;
                configuration.HeaderMode = UICollectionLayoutListHeaderMode.FirstItemInSection;
                return NSCollectionLayoutSection.GetSection(configuration, layoutEnvironment);
            });
        }

        private void ConfigureDataSource()
        {
            var headerRegistration = UICollectionViewCellRegistration.GetRegistration(typeof(UICollectionViewListCell),
                 new UICollectionViewCellRegistrationConfigurationHandler((cell, indexpath, item) =>
                 {
                     var sidebarItem = (SidebarItem)item;
                     var contentConfiguration = UIListContentConfiguration.SidebarHeaderConfiguration;
                     contentConfiguration.Text = sidebarItem.Title;
                     contentConfiguration.TextProperties.Font = UIFont.PreferredSubheadline;
                     contentConfiguration.TextProperties.Color = UIColor.SecondaryLabel;
                     cell.ContentConfiguration = contentConfiguration;
                     ((UICollectionViewListCell)cell).Accessories = new[] { new UICellAccessoryOutlineDisclosure() };
                 }));

            var expandableRowRegistration = UICollectionViewCellRegistration.GetRegistration(typeof(UICollectionViewListCell),
                 new UICollectionViewCellRegistrationConfigurationHandler((cell, indexpath, item) =>
                 {
                     var sidebarItem = (SidebarItem)item;
                     var contentConfiguration = UIListContentConfiguration.SidebarSubtitleCellConfiguration;
                     contentConfiguration.Text = sidebarItem.Title;
                     contentConfiguration.SecondaryText = sidebarItem.Subtitle;
                     contentConfiguration.Image = sidebarItem.Image;
                     contentConfiguration.TextProperties.Font = UIFont.PreferredSubheadline;
                     contentConfiguration.TextProperties.Color = UIColor.SecondaryLabel;

                     cell.ContentConfiguration = contentConfiguration;
                     ((UICollectionViewListCell)cell).Accessories = new[] { new UICellAccessoryOutlineDisclosure() };
                 }));

            var rowRegistration = UICollectionViewCellRegistration.GetRegistration(typeof(UICollectionViewListCell),
                 new UICollectionViewCellRegistrationConfigurationHandler((cell, indexpath, item) =>
                 {
                     var sidebarItem = (SidebarItem)item;
                     var contentConfiguration = UIListContentConfiguration.SidebarSubtitleCellConfiguration;
                     contentConfiguration.Text = sidebarItem.Title;
                     contentConfiguration.SecondaryText = sidebarItem.Subtitle;
                     contentConfiguration.Image = sidebarItem.Image;
                     contentConfiguration.TextProperties.Font = UIFont.PreferredSubheadline;
                     contentConfiguration.TextProperties.Color = UIColor.SecondaryLabel;

                     cell.ContentConfiguration = contentConfiguration;
                 }));

            this.dataSource = new UICollectionViewDiffableDataSource<NSString, SidebarItem>(collectionView!,
                new UICollectionViewDiffableDataSourceCellProvider((collectionView, indexPath, item) =>
                {
                    var sidebarItem = (SidebarItem)item;

                    switch (sidebarItem.Type)
                    {
                        case SidebarItemType.Header:
                            return collectionView.DequeueConfiguredReusableCell(headerRegistration, indexPath, item);
                        case SidebarItemType.ExpandableRow:
                            return collectionView.DequeueConfiguredReusableCell(expandableRowRegistration, indexPath, item);
                        default:
                            return collectionView.DequeueConfiguredReusableCell(rowRegistration, indexPath, item);
                    }
                })
            );
        }

        private class SidebarItem : NSObject
        {
            public Guid Id { get; }

            public SidebarItemType Type { get; }

            public string Title { get; }

            public string? Subtitle { get; }

            public UIImage? Image { get; }

            public SidebarItem(Guid id, SidebarItemType type, string title, string? subtitle = default, UIImage? image = default)
            {
                this.Id = id;
                this.Type = type;
                this.Title = title;
                this.Subtitle = subtitle;
                this.Image = image;
            }

            public static SidebarItem Header(string title, Guid? id = default)
                => new SidebarItem(id ?? Guid.NewGuid(), SidebarItemType.Header, title);

            public static SidebarItem ExpandableRow(string title, string? subtitle = default, UIImage? image = default, Guid? id = default)
             => new SidebarItem(id ?? Guid.NewGuid(), SidebarItemType.ExpandableRow, title, subtitle, image);

            public static SidebarItem Row(string title, string? subtitle = default, UIImage? image = default, Guid? id = default)
                => new SidebarItem(id ?? Guid.NewGuid(), SidebarItemType.Row, title, subtitle, image);
        }

        private enum SidebarItemType
        {
            Header,
            ExpandableRow,
            Row,
        }

        private enum SidebarSection
        {
            SmartFeeds,
            Local,
        }
    }
}
#endif