// <copyright file="SidebarViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.PureLayout;
using MauiFeed.MacCatalyst.Sidebar;
using MauiFeed.Models;
using MauiFeed.Services;
using Microsoft.EntityFrameworkCore;
using MobileCoreServices;

namespace MauiFeed.MacCatalyst.ViewControllers
{
    /// <summary>
    /// Sidebar View Controller.
    /// </summary>
    public class SidebarViewController : UIViewController, IUICollectionViewDelegate, IUICollectionViewDragDelegate
    {
        private RootSplitViewController rootSplitViewController;

        private DatabaseContext databaseContext;
        private UICollectionView? collectionView;
        private UICollectionViewDiffableDataSource<NSString, SidebarItem>? dataSource;
        private List<SidebarItem> smartFilterItems;
        private List<SidebarItem> sidebarItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="SidebarViewController"/> class.
        /// </summary>
        /// <param name="controller">Root View Controller.</param>
        public SidebarViewController(RootSplitViewController controller)
        {
            this.smartFilterItems = new List<SidebarItem>();
            this.sidebarItems = new List<SidebarItem>();
            this.rootSplitViewController = controller;
            this.databaseContext = (DatabaseContext)Ioc.Default.GetService<DatabaseContext>()!;
        }

        private NSString SmartFilterIdentifier => new NSString(SidebarItemType.SmartFilter.ToString());

        private NSString FeedListItemIdentifier => new NSString(SidebarItemType.FeedListItem.ToString());

        /// <inheritdoc/>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.ConfigureCollectionView();
            this.ConfigureDataSource();
            this.GenerateSidebar();
        }

        /// <summary>
        /// Update the sidebar.
        /// </summary>
        public void UpdateSidebar()
        {
            var smartFilter = this.dataSource!.GetSnapshot(this.SmartFilterIdentifier);
            foreach (var item in smartFilter.Items)
            {
                item.Update();
            }

            var feedList = this.dataSource!.GetSnapshot(this.FeedListItemIdentifier);
            foreach (var item in feedList.Items)
            {
                item.Update();
            }
        }

        /// <summary>
        /// Generate the sidebar.
        /// </summary>
        public void GenerateSidebar()
        {
            this.smartFilterItems.Clear();
            this.dataSource!.ApplySnapshot(this.ConfigureSmartFeedSnapshot(), this.SmartFilterIdentifier, false);
            this.dataSource!.ApplySnapshot(this.ConfigureLocalSnapshot(), this.FeedListItemIdentifier, false);
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
#pragma warning disable CA1416 // プラットフォームの互換性を検証
#pragma warning disable CA1422 // プラットフォームの互換性を検証
            itemProvider.RegisterDataRepresentation(UTType.PlainText, NSItemProviderRepresentationVisibility.OwnProcess, (completionHandler) =>
            {
#nullable disable
                completionHandler(data, null);
                return null;
#nullable enable
            });

#pragma warning restore CA1422 // プラットフォームの互換性を検証
#pragma warning restore CA1416 // プラットフォームの互換性を検証

            return new UIDragItem[] { new UIDragItem(itemProvider) { LocalObject = listCell } };
        }

        /// <summary>
        /// Drag Session.
        /// </summary>
        /// <param name="collectionView">Collection View.</param>
        /// <param name="session">Session.</param>
        /// <returns>Bool.</returns>
        [Export("collectionView:dragSessionIsRestrictedToDraggingApplication:")]
        public bool DragSessionIsRestrictedToDraggingApplication(UICollectionView collectionView, IUIDragSession session)
        {
            return true;
        }

        [Export("collectionView:dragSessionWillBegin:")]
#pragma warning disable SA1600 // Elements should be documented
        public void DragSessionWillBegin(UICollectionView collectionView, IUIDragSession session)
        {
        }

        [Export("collectionView:dragSessionDidEnd:")]
        public void DragSessionDidEnd(UICollectionView collectionView, IUIDragSession session)
        {
        }
#pragma warning restore SA1600 // Elements should be documented

        /// <summary>
        /// Fired when Item is Selected.
        /// </summary>
        /// <param name="collectionView">CollectionView.</param>
        /// <param name="indexPath">Index Path.</param>
        [Export("collectionView:didSelectItemAtIndexPath:")]
        protected void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            this.rootSplitViewController.FeedCollection.SidebarItem = this.dataSource?.GetItemIdentifier(indexPath);
        }

        private NSDiffableDataSourceSectionSnapshot<SidebarItem> ConfigureSmartFeedSnapshot()
        {
            var snapshot = new NSDiffableDataSourceSectionSnapshot<SidebarItem>();
            var header = new SidebarItem("Smart Feeds", SidebarItemRowType.Header, SidebarItemType.SmartFilter);
            var items = new SidebarItem[]
            {
                new SidebarItem(Translations.Common.AllLabel, UIImage.GetSystemImage("newspaper.circle")!, this.databaseContext.FeedItems!.Include(n => n.Feed)),
                new SidebarItem(Translations.Common.TodayLabel, UIImage.GetSystemImage("sun.max")!, this.databaseContext.FeedItems!.Include(n => n.Feed).Where(n => n.PublishingDate != null && n.PublishingDate!.Value.Date == DateTime.UtcNow.Date)),
                new SidebarItem(Translations.Common.AllUnreadLabel, UIImage.GetSystemImage("circle.inset.filled")!, this.databaseContext.FeedItems!.Include(n => n.Feed).Where(n => !n.IsRead)),
                new SidebarItem(Translations.Common.StarredLabel, UIImage.GetSystemImage("star.fill")!, this.databaseContext.FeedItems!.Include(n => n.Feed).Where(n => n.IsFavorite)),
            };

            snapshot.AppendItems(new[] { header });
            snapshot.ExpandItems(new[] { header });
            snapshot.AppendItems(items, header);

            return snapshot;
        }

        private NSDiffableDataSourceSectionSnapshot<SidebarItem> ConfigureLocalSnapshot()
        {
            this.sidebarItems.Clear();
            var snapshot = new NSDiffableDataSourceSectionSnapshot<SidebarItem>();

            var items = new List<SidebarItem>();

            var header = new SidebarItem(Translations.Common.FeedLabel, SidebarItemRowType.Header, SidebarItemType.FeedListItem);

            // Individual Items.
            foreach (var item in this.databaseContext.FeedListItems!.Include(n => n.Items).Where(n => n.FolderId == null || n.FolderId <= 0))
            {
                items.Add(new SidebarItem(item, this.databaseContext.FeedItems!.Include(n => n.Feed).Where(n => n.FeedListItemId == item.Id)));
            }

            snapshot.AppendItems(new[] { header });
            snapshot.ExpandItems(new[] { header });

            foreach (var item in this.databaseContext.FeedFolder!.Include(n => n.Items)!)
            {
                var folder = new SidebarItem(item, this.databaseContext.FeedItems!.Include(n => n.Feed).Where(n => (n.Feed!.FolderId ?? 0) == item.Id));
                snapshot.AppendItems(new[] { folder }, header);
                snapshot.ExpandItems(new[] { folder });

                var folderItems = new List<SidebarItem>();

                foreach (var folderItem in item.Items!)
                {
                    folderItems.Add(new SidebarItem(folderItem, this.databaseContext.FeedItems!.Include(n => n.Feed).Where(n => n.FeedListItemId == item.Id)));
                }

                snapshot.AppendItems(folderItems.ToArray(), folder);
                snapshot.CollapseItems(new[] { folder });
            }

            snapshot.AppendItems(items.ToArray(), header);
            return snapshot;
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
                typeof(SidebarListCell),
                new UICollectionViewCellRegistrationConfigurationHandler((cell, indexpath, item) =>
                {
                    var sidebarItem = (SidebarItem)item;
                    var listCell = (SidebarListCell)cell;
                    var contentConfiguration = UIListContentConfiguration.SidebarSubtitleCellConfiguration;
                    contentConfiguration.Text = sidebarItem.Title;
                    contentConfiguration.Image = sidebarItem.Image;
                    contentConfiguration.TextProperties.Font = UIFont.PreferredSubheadline;
                    contentConfiguration.TextProperties.Color = UIColor.SecondaryLabel;

                    listCell.ContentConfiguration = contentConfiguration;
                    ((UICollectionViewListCell)cell).Accessories = new[] { new UICellAccessoryOutlineDisclosure() };
                    listCell.SetupCell(sidebarItem);
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

                    switch (sidebarItem.RowType)
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
    }
}