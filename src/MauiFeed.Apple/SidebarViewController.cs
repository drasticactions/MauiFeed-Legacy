#if !TVOS
// <copyright file="SidebarViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Linq.Expressions;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.PureLayout;
using MauiFeed.Models;
using MauiFeed.Services;
using ObjCRuntime;

namespace MauiFeed.Apple
{
    public class SidebarViewController : UIViewController, IUICollectionViewDelegate
    {
        private RootSplitViewController rootSplitViewController;
        private EFCoreDatabaseContext databaseContext;
        private Guid smartFilterRowIdentifier = Guid.NewGuid();
        private Guid localRowIdentifier = Guid.NewGuid();

        private UICollectionViewDiffableDataSource<NSString, SidebarItem>? dataSource;
        private UICollectionView? collectionView;

        public SidebarViewController(RootSplitViewController controller)
        {
            this.rootSplitViewController = controller;
            this.databaseContext = (EFCoreDatabaseContext)Ioc.Default.GetService<IDatabaseService>()!;
            this.databaseContext.OnDatabaseUpdated += this.DatabaseContext_OnDatabaseUpdated;
        }

        private void DatabaseContext_OnDatabaseUpdated(object? sender, EventArgs e)
        {
            this.ApplyInitialSnapshot();
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
            this.rootSplitViewController.FeedTableViewController.Update(sidebarItem!.Items.ToArray());
#if IOS
            this.rootSplitViewController.ShowColumn(UISplitViewControllerColumn.Supplementary);
#endif
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
            var header = SidebarItem.Header(this.databaseContext, "Smart Feeds");
            var items = new SidebarItem[]
            {
                SidebarItem.Row(this.databaseContext, "Today", null, UIImage.GetSystemImage("sun.max"), filter: this.databaseContext.CreateFilter<FeedItem, DateTime?>(o => o.PublishingDate, DateTime.UtcNow.Date, EFCoreDatabaseContext.FilterType.GreaterThanOrEqual)),
                SidebarItem.Row(this.databaseContext, "All Unread", null, UIImage.GetSystemImage("circle.inset.filled"), filter: this.databaseContext.CreateFilter<FeedItem, bool>(o => o.IsRead, false, EFCoreDatabaseContext.FilterType.Equals)),
                SidebarItem.Row(this.databaseContext, "Starred", null, UIImage.GetSystemImage("star.fill")),
            };

            snapshot.AppendItems(new[] { header });
            snapshot.ExpandItems(new[] { header });
            snapshot.AppendItems(items, header);
            return snapshot;
        }

        private NSDiffableDataSourceSectionSnapshot<SidebarItem> ConfigureLocalSnapshot()
        {
            var snapshot = new NSDiffableDataSourceSectionSnapshot<SidebarItem>();
            var header = SidebarItem.Header(this.databaseContext, "Local");

            var items = new List<SidebarItem>();

            foreach (var item in this.databaseContext.FeedListItems!)
            {
                var test = this.databaseContext.CreateFilter<FeedItem, int>(o => o.FeedListItemId, item.Id, EFCoreDatabaseContext.FilterType.Equals);
                items.Add(SidebarItem.Row(this.databaseContext, item.Name!, null, UIImage.LoadFromData(NSData.FromArray(item.ImageCache!))!.Scale(new CGSize(16, 16), 2f), filter: test));
            }

            snapshot.AppendItems(new[] { header });
            snapshot.ExpandItems(new[] { header });
            snapshot.AppendItems(items.ToArray(), header);
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

        private static UIColor GetSystemTint()
        {
#if TVOS
            return UIColor.Clear;
#else
            var poop = UIConfigurationColorTransformer.PreferredTint;
            return poop(UIColor.Clear);
#endif
        }

        private class PaddingLabel : UILabel
        {
            public PaddingLabel()
            {
                this.Layer.MasksToBounds = true;
                this.Layer.CornerRadius = 5f;
                this.BackgroundColor = GetSystemTint();
                this.Font = this.Font.WithSize(10);
            }

            private UIEdgeInsets textEdgeInsets = UIEdgeInsets.Zero;

            public UIEdgeInsets TextEdgeInsets
            {
                get => textEdgeInsets;
                set
                {
                    textEdgeInsets = value;
                    InvalidateIntrinsicContentSize();
                }
            }

            public override CGRect TextRectForBounds(CGRect bounds, nint numberOfLines)
            {
                var insetRect = textEdgeInsets.InsetRect(bounds);
                var textRect = base.TextRectForBounds(insetRect, numberOfLines);
                var invertedInsets = new UIEdgeInsets(-textEdgeInsets.Top, -textEdgeInsets.Left, -textEdgeInsets.Bottom, -textEdgeInsets.Right);
                return invertedInsets.InsetRect(textRect);
            }

            public override void DrawText(CGRect rect)
            {
                base.DrawText(textEdgeInsets.InsetRect(rect));
            }
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
                     var listCell = (UICollectionViewListCell)cell;
                     var sidebarItem = (SidebarItem)item;
                     var contentConfiguration = UIListContentConfiguration.SidebarSubtitleCellConfiguration;
                     contentConfiguration.Text = sidebarItem.Title;
                     contentConfiguration.SecondaryText = sidebarItem.Subtitle;
                     contentConfiguration.Image = sidebarItem.Image;
                     contentConfiguration.TextProperties.Font = UIFont.PreferredSubheadline;
                     contentConfiguration.TextProperties.Color = UIColor.SecondaryLabel;
                     if (sidebarItem.UnreadCount > 0)
                     {
                         var holder = new PaddingLabel() { Text = sidebarItem.UnreadCount.ToString() };
                         holder.TextEdgeInsets = new UIEdgeInsets(2, 5, 2, 5);
                         var test2 = new UICellAccessoryCustomView(holder, UICellAccessoryPlacement.Trailing);
                         ((UICollectionViewListCell)cell).Accessories = new UICellAccessory[] { test2 };
                     }
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
            private EFCoreDatabaseContext context;

            public Guid Id { get; }

            public SidebarItemType Type { get; }

            public string Title { get; }

            public string? Subtitle { get; }

            public UIImage? Image { get; }

            public SidebarItem(Guid id, EFCoreDatabaseContext context, SidebarItemType type, string title, string? subtitle = default, UIImage? image = default, Expression<Func<FeedItem, bool>>? filter = default)
            {
                this.Id = id;
                this.Type = type;
                this.Title = title;
                this.Subtitle = subtitle;
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
                        return this.context.FeedItems!.Where(this.Filter).OrderByDescending(n => n.PublishingDate).ToList();
                    }

                    return new List<FeedItem>();
                }
            }

            public async Task Update()
            {

            }

            public static SidebarItem Header(EFCoreDatabaseContext context, string title, Guid? id = default, Expression<Func<FeedItem, bool>>? filter = default)
                => new SidebarItem(id ?? Guid.NewGuid(), context, SidebarItemType.Header, title, filter: filter);

            public static SidebarItem ExpandableRow(EFCoreDatabaseContext context, string title, string? subtitle = default, UIImage? image = default, Guid? id = default, Expression<Func<FeedItem, bool>>? filter = default)
             => new SidebarItem(id ?? Guid.NewGuid(), context, SidebarItemType.ExpandableRow, title, subtitle, image, filter);

            public static SidebarItem Row(EFCoreDatabaseContext context, string title, string? subtitle = default, UIImage? image = default, Guid? id = default, Expression<Func<FeedItem, bool>>? filter = default)
                => new SidebarItem(id ?? Guid.NewGuid(), context, SidebarItemType.Row, title, subtitle, image, filter);
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