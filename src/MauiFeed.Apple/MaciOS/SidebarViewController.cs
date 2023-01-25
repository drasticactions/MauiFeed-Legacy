// <copyright file="SidebarViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.PureLayout;
using MauiFeed.Models;
using MauiFeed.Services;
using MauiFeed.Views;
using MobileCoreServices;

namespace MauiFeed.Apple
{
    public class SidebarViewController : UIViewController, IUICollectionViewDelegate, IUICollectionViewDragDelegate, Views.ISidebarView
    {
        private RootSplitViewController rootSplitViewController;

        private DatabaseContext databaseContext;
        private UICollectionView? collectionView;
        private UICollectionViewDiffableDataSource<NSString, SidebarItem>? dataSource;
        private List<SidebarItem> smartFilterItems;
        private List<SidebarItem> sidebarItems;
        private Guid smartFilterRowIdentifier = Guid.NewGuid();
        private Guid localRowIdentifier = Guid.NewGuid();
        private SidebarItem? selectedItem;

        public SidebarViewController(RootSplitViewController controller)
        {
            this.smartFilterItems = new List<SidebarItem>();
            this.sidebarItems = new List<SidebarItem>();

            this.rootSplitViewController = controller;
            this.databaseContext = (DatabaseContext)Ioc.Default.GetService<DatabaseContext>()!;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.ConfigureCollectionView();
            this.ConfigureDataSource();
            this.GenerateSidebar();
        }

        /// <inheritdoc/>
        public void AddItemToSidebar(FeedListItem item)
        {
            this.GenerateSidebar();
        }

        /// <inheritdoc/>
        public void GenerateSidebar()
        {
            this.smartFilterItems.Clear();
            this.sidebarItems.Clear();
            this.dataSource!.ApplySnapshot(this.ConfigureSmartFeedSnapshot(), new NSString(SidebarSection.SmartFeeds.ToString()), false);
            this.dataSource!.ApplySnapshot(this.ConfigureLocalSnapshot(), new NSString(SidebarSection.Local.ToString()), false);
        }

        /// <inheritdoc/>
        public void UpdateSidebar()
        {
            Task.Run(() =>
            {
                foreach (var item in this.smartFilterItems)
                {
                    item.Update();
                }

                foreach (var item in this.sidebarItems)
                {
                    item.Update();
                }
            });
        }

        /// <inheritdoc/>
        public UIDragItem[] GetItemsForBeginningDragSession(UICollectionView collectionView, IUIDragSession session, NSIndexPath indexPath)
        {
            var item = this.dataSource?.GetCell(collectionView, indexPath) as SidebarListCell;

            if (item is not SidebarListCell listCell)
            {
                return new UIDragItem[] { };
            }

            // set "sectionIndex,rowIndex" as string
            var data = NSData.FromString($"{indexPath.Section},{indexPath.Row}");

            var itemProvider = new NSItemProvider();
            itemProvider.RegisterDataRepresentation(UTType.PlainText, NSItemProviderRepresentationVisibility.All, (completionHandler) =>
            {
#nullable disable
                completionHandler(data, null);
                return null;
#nullable enable
            });

            return new UIDragItem[] { new UIDragItem(itemProvider) };
        }

/* プロジェクト 'MauiFeed.Apple(net7.0-maccatalyst)' からのマージされていない変更
追加済み:
        public Task MoveItemToFolder(ISidebarItem item, ISidebarItem folder)
        {
            throw new NotImplementedException();
        }
*/

/* プロジェクト 'MauiFeed.Apple(net7.0-maccatalyst)' からのマージされていない変更
前:
            this.collectionView.DragDelegate = this; 
後:
            this.collectionView.DragDelegate = this;
*/

/* プロジェクト 'MauiFeed.Apple(net7.0-maccatalyst)' からのマージされていない変更
前:
                })
            );
後:
                }));
*/

/* プロジェクト 'MauiFeed.Apple(net7.0-maccatalyst)' からのマージされていない変更
削除済み:
        public Task MoveItemToFolder(ISidebarItem item, ISidebarItem folder)
        {
            throw new NotImplementedException();
        }
*/

        public Task MoveItemToFolder(ISidebarItem item, ISidebarItem folder)
        {
            throw new NotImplementedException();
        }

        [Export("collectionView:didSelectItemAtIndexPath:")]
        protected void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            this.selectedItem = this.dataSource?.GetItemIdentifier(indexPath);
            this.rootSplitViewController.FeedViewController.SetFeed(this.selectedItem!);
#if IOS
            this.rootSplitViewController.ShowColumn(UISplitViewControllerColumn.Supplementary);
#endif
        }

        private void ConfigureCollectionView()
        {
            this.collectionView = new UICollectionView(this.View!.Bounds, this.CreateLayout());
            this.collectionView.Delegate = this;
            this.collectionView.DragDelegate = this;
            this.collectionView.BackgroundColor = UIColor.SystemBackground;
            this.View.AddSubview(this.collectionView);
            this.collectionView.TranslatesAutoresizingMaskIntoConstraints = false;
            this.collectionView.AutoPinEdgesToSuperviewEdges();
        }

        private NSDiffableDataSourceSectionSnapshot<SidebarItem> ConfigureSmartFeedSnapshot()
        {
            var snapshot = new NSDiffableDataSourceSectionSnapshot<SidebarItem>();
            var header = SidebarItem.Header(this.databaseContext, "Smart Feeds", SidebarSection.SmartFeeds, this.smartFilterRowIdentifier);
            var items = new SidebarItem[]
            {
                SidebarItem.Row(this.databaseContext, Translations.Common.AllLabel, SidebarSection.SmartFeeds, null, UIImage.GetSystemImage("newspaper.circle"), filter: this.databaseContext.CreateFilter<FeedItem, int>(o => o.Id, 0, DatabaseContext.FilterType.GreaterThan), type: SidebarItemType.SmartFilter),
                SidebarItem.Row(this.databaseContext, Translations.Common.TodayLabel, SidebarSection.SmartFeeds, null, UIImage.GetSystemImage("sun.max"), filter: this.databaseContext.CreateFilter<FeedItem, DateTime?>(o => o.PublishingDate, DateTime.UtcNow.Date, DatabaseContext.FilterType.GreaterThanOrEqual), type: SidebarItemType.SmartFilter),
                SidebarItem.Row(this.databaseContext, Translations.Common.AllUnreadLabel, SidebarSection.SmartFeeds, null, UIImage.GetSystemImage("circle.inset.filled"), filter: this.databaseContext.CreateFilter<FeedItem, bool>(o => o.IsRead, false, DatabaseContext.FilterType.Equals), type: SidebarItemType.SmartFilter),
                SidebarItem.Row(this.databaseContext, Translations.Common.StarredLabel, SidebarSection.SmartFeeds, null, UIImage.GetSystemImage("star.fill"), filter: this.databaseContext.CreateFilter<FeedItem, bool>(o => o.IsFavorite, true, DatabaseContext.FilterType.Equals), type: SidebarItemType.SmartFilter),
            };

            snapshot.AppendItems(new[] { header });
            snapshot.ExpandItems(new[] { header });
            snapshot.AppendItems(items, header);

            this.smartFilterItems.AddRange(items);
            return snapshot;
        }

        private NSDiffableDataSourceSectionSnapshot<SidebarItem> ConfigureLocalSnapshot()
        {
            var snapshot = new NSDiffableDataSourceSectionSnapshot<SidebarItem>();
            var header = SidebarItem.Header(this.databaseContext, "Local", SidebarSection.Local, this.localRowIdentifier);

            var items = new List<SidebarItem>();

            foreach (var item in this.databaseContext.FeedListItems!)
            {
                var test = this.databaseContext.CreateFilter<FeedItem, int>(o => o.FeedListItemId, item.Id, DatabaseContext.FilterType.Equals);
                items.Add(SidebarItem.Row(this.databaseContext, item.Name!, SidebarSection.Local, null, UIImage.LoadFromData(NSData.FromArray(item.ImageCache!))!.Scale(new CGSize(16, 16), 2f), filter: test, type: SidebarItemType.FeedListItem));
            }

            snapshot.AppendItems(new[] { header });
            snapshot.ExpandItems(new[] { header });
            snapshot.AppendItems(items.ToArray(), header);
            this.sidebarItems.AddRange(items);
            return snapshot;
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
            var headerRegistration = UICollectionViewCellRegistration.GetRegistration(
                typeof(UICollectionViewListCell),
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

            var expandableRowRegistration = UICollectionViewCellRegistration.GetRegistration(
                typeof(UICollectionViewListCell),
                new UICollectionViewCellRegistrationConfigurationHandler((cell, indexpath, item) =>
                 {
                     var sidebarItem = (SidebarItem)item;
                     var contentConfiguration = UIListContentConfiguration.SidebarSubtitleCellConfiguration;
                     contentConfiguration.Text = sidebarItem.Title;
                     contentConfiguration.Image = sidebarItem.Image;
                     contentConfiguration.TextProperties.Font = UIFont.PreferredSubheadline;
                     contentConfiguration.TextProperties.Color = UIColor.SecondaryLabel;

                     cell.ContentConfiguration = contentConfiguration;
                     ((UICollectionViewListCell)cell).Accessories = new[] { new UICellAccessoryOutlineDisclosure() };
                 }));

            var rowRegistration = UICollectionViewCellRegistration.GetRegistration(
                typeof(SidebarListCell),
                new UICollectionViewCellRegistrationConfigurationHandler((cell, indexpath, item) =>
                 {
                     var listCell = (SidebarListCell)cell;
                     listCell.SetupCell((SidebarItem)item);
                 }));

            this.dataSource = new UICollectionViewDiffableDataSource<NSString, SidebarItem>(
                this.collectionView!,
                new UICollectionViewDiffableDataSourceCellProvider((collectionView, indexPath, item) =>
                {
                    var sidebarItem = (SidebarItem)item;

                    switch (sidebarItem.Type)
                    {
                        case SidebarItemRowType.Header:
                            return collectionView.DequeueConfiguredReusableCell(headerRegistration, indexPath, item);
                        case SidebarItemRowType.ExpandableRow:
                            return collectionView.DequeueConfiguredReusableCell(expandableRowRegistration, indexPath, item);
                        default:
                            return collectionView.DequeueConfiguredReusableCell(rowRegistration, indexPath, item);
                    }
                }));
        }

        public Task RemoveFromFolder(ISidebarItem item, bool moveToRoot = false)
        {
            throw new NotImplementedException();
        }
    }
}