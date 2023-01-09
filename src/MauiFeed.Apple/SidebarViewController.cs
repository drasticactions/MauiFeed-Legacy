#if !TVOS
// <copyright file="SidebarViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.PureLayout;
using MauiFeed.Models;
using MauiFeed.Services;
using Microsoft.EntityFrameworkCore;
using ObjCRuntime;
using static System.Net.Mime.MediaTypeNames;

namespace MauiFeed.Apple
{
    public class SidebarViewController : UIViewController, IUICollectionViewDelegate, Views.ISidebarView
    {
        private RootSplitViewController rootSplitViewController;
        private EFCoreDatabaseContext databaseContext;
        private Guid smartFilterRowIdentifier = Guid.NewGuid();
        private Guid localRowIdentifier = Guid.NewGuid();

        private UICollectionViewDiffableDataSource<NSString, SidebarItem>? dataSource;
        private UICollectionView? collectionView;
        private List<SidebarItem> SidebarItems;

        public SidebarViewController(RootSplitViewController controller)
        {
            this.SidebarItems = new List<SidebarItem>();
            this.rootSplitViewController = controller;
            this.databaseContext = (EFCoreDatabaseContext)Ioc.Default.GetService<IDatabaseService>()!;
            this.databaseContext.OnDatabaseUpdated += this.DatabaseContext_OnDatabaseUpdated;
        }

        private void DatabaseContext_OnDatabaseUpdated(object? sender, EventArgs e)
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
            this.rootSplitViewController.FeedTableViewController.SetFeedItems(sidebarItem!.Items);
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
            var header = SidebarItem.Header(this.databaseContext, "Smart Feeds", this.smartFilterRowIdentifier);
            var items = new SidebarItem[]
            {
                SidebarItem.Row(this.databaseContext, "Today", null, UIImage.GetSystemImage("sun.max"), filter: this.databaseContext.CreateFilter<FeedItem, DateTime?>(o => o.PublishingDate, DateTime.UtcNow.Date, EFCoreDatabaseContext.FilterType.GreaterThanOrEqual)),
                SidebarItem.Row(this.databaseContext, "All Unread", null, UIImage.GetSystemImage("circle.inset.filled"), filter: this.databaseContext.CreateFilter<FeedItem, bool>(o => o.IsRead, false, EFCoreDatabaseContext.FilterType.Equals)),
                SidebarItem.Row(this.databaseContext, "Starred", null, UIImage.GetSystemImage("star.fill"), filter: this.databaseContext.CreateFilter<FeedItem, bool>(o => o.IsFavorite, true, EFCoreDatabaseContext.FilterType.Equals)),
            };

            snapshot.AppendItems(new[] { header });
            snapshot.ExpandItems(new[] { header });
            snapshot.AppendItems(items, header);

            this.SidebarItems.AddRange(items);
            return snapshot;
        }

        private NSDiffableDataSourceSectionSnapshot<SidebarItem> ConfigureLocalSnapshot()
        {
            var snapshot = new NSDiffableDataSourceSectionSnapshot<SidebarItem>();
            var header = SidebarItem.Header(this.databaseContext, "Local", this.localRowIdentifier);

            var items = new List<SidebarItem>();

            foreach (var item in this.databaseContext.FeedListItems!)
            {
                var test = this.databaseContext.CreateFilter<FeedItem, int>(o => o.FeedListItemId, item.Id, EFCoreDatabaseContext.FilterType.Equals);
                items.Add(SidebarItem.Row(this.databaseContext, item.Name!, null, UIImage.LoadFromData(NSData.FromArray(item.ImageCache!))!.Scale(new CGSize(16, 16), 2f), filter: test));
            }

            snapshot.AppendItems(new[] { header });
            snapshot.ExpandItems(new[] { header });
            snapshot.AppendItems(items.ToArray(), header);
            this.SidebarItems.AddRange(items);
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
                     contentConfiguration.Image = sidebarItem.Image;
                     contentConfiguration.TextProperties.Font = UIFont.PreferredSubheadline;
                     contentConfiguration.TextProperties.Color = UIColor.SecondaryLabel;

                     cell.ContentConfiguration = contentConfiguration;
                     ((UICollectionViewListCell)cell).Accessories = new[] { new UICellAccessoryOutlineDisclosure() };
                 }));

            var rowRegistration = UICollectionViewCellRegistration.GetRegistration(typeof(SidebarListCell),
                 new UICollectionViewCellRegistrationConfigurationHandler((cell, indexpath, item) =>
                 {
                     var listCell = (SidebarListCell)cell;
                     listCell.SetupCell((SidebarItem)item);
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

        private class SidebarItem : NSObject, INotifyPropertyChanged
        {
            private EFCoreDatabaseContext context;

            public Guid Id { get; }

            public SidebarItemType Type { get; }

            public string Title { get; }

            public UIImage? Image { get; }

            public SidebarItem(Guid id, EFCoreDatabaseContext context, SidebarItemType type, string title, UIImage? image = default, Expression<Func<FeedItem, bool>>? filter = default)
            {
                this.Id = id;
                this.Type = type;
                this.Title = title;
                this.Image = image;
                this.Filter = filter;
                this.context = context;
            }

            /// <inheritdoc/>
            public event PropertyChangedEventHandler? PropertyChanged;

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
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SidebarItem"));
            }

            public static SidebarItem Header(EFCoreDatabaseContext context, string title, Guid? id = default, Expression<Func<FeedItem, bool>>? filter = default)
                => new SidebarItem(id ?? Guid.NewGuid(), context, SidebarItemType.Header, title, filter: filter);

            public static SidebarItem ExpandableRow(EFCoreDatabaseContext context, string title, UIImage? image = default, Guid? id = default, Expression<Func<FeedItem, bool>>? filter = default)
             => new SidebarItem(id ?? Guid.NewGuid(), context, SidebarItemType.ExpandableRow, title, image, filter);

            public static SidebarItem Row(EFCoreDatabaseContext context, string title, FeedListItem? subtitle = default, UIImage? image = default, Guid? id = default, Expression<Func<FeedItem, bool>>? filter = default)
                => new SidebarItem(id ?? Guid.NewGuid(), context, SidebarItemType.Row, title, image, filter);
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

        private class SidebarListCell : UICollectionViewListCell
        {
            private SidebarItem? item;
            private PaddingLabel unreadLabel;
            private UIListContentConfiguration config;

            protected internal SidebarListCell(NativeHandle handle)
               : base(handle)
            {
                this.unreadLabel = new PaddingLabel() { };
                this.unreadLabel.TextEdgeInsets = new UIEdgeInsets(2, 5, 2, 5);
                this.ContentConfiguration = this.config = UIListContentConfiguration.SidebarSubtitleCellConfiguration;
            }

            public void SetupCell(SidebarItem item)
            {
                if (this.item is not null)
                {
                    this.item.PropertyChanged -= Item_PropertyChanged;
                }

                this.item = item;
                this.item.PropertyChanged += Item_PropertyChanged;
                this.config.Text = this.item.Title;
                this.config.SecondaryText = this.item.Subtitle;
                this.config.Image = this.item.Image;
                this.config.TextProperties.Font = UIFont.PreferredSubheadline;
                this.config.TextProperties.Color = UIColor.SecondaryLabel;
                this.UpdateIsRead();
                this.ContentConfiguration = this.config;
            }

            private void UpdateIsRead()
            {
                if (this.item?.UnreadCount > 0)
                {
                    this.unreadLabel.Text = this.item?.UnreadCount.ToString();
                    var test2 = new UICellAccessoryCustomView(this.unreadLabel, UICellAccessoryPlacement.Trailing);
                    ((UICollectionViewListCell)this).Accessories = new UICellAccessory[] { test2 };
                }
                else
                {
                    ((UICollectionViewListCell)this).Accessories = new UICellAccessory[] { };
                }
            }

            private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                this.InvokeOnMainThread(this.UpdateIsRead);
            }
        }
    }
}
#endif