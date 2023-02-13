// <copyright file="TimelineCollectionViewController.cs" company="Drastic Actions">
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
using MauiFeed.Views;
using ObjCRuntime;

namespace MauiFeed.MacCatalyst.ViewControllers
{
    /// <summary>
    /// Timeline Collection View Controller.
    /// </summary>
    public class TimelineCollectionViewController
        : UIViewController, IUICollectionViewDelegate
    {
        private SidebarItem? sidebarItem;
        private UICollectionView collectionView;
        private UICollectionViewDiffableDataSource<NSString, MacFeedItem> dataSource;
        private RootSplitViewController controller;
        private DatabaseContext database;
        private FeedItem? selectedItem;
        private IErrorHandlerService errorHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineCollectionViewController"/> class.
        /// </summary>
        /// <param name="controller">Root View Controller.</param>
        public TimelineCollectionViewController(RootSplitViewController controller)
        {
            this.database = (DatabaseContext)Ioc.Default.GetService<DatabaseContext>()!;
            this.errorHandler = (IErrorHandlerService)Ioc.Default.GetService<IErrorHandlerService>()!;
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

            this.View.AddSubview(this.collectionView);
            this.collectionView.TranslatesAutoresizingMaskIntoConstraints = false;
            this.collectionView.AutoPinEdgesToSuperviewEdges();
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
            this.dataSource!.ApplySnapshot(this.ConfigureLocalSnapshot(), new NSString(string.Empty), false);
        }

        /// <summary>
        /// Fired when Item is Selected.
        /// </summary>
        /// <param name="collectionView">CollectionView.</param>
        /// <param name="indexPath">Index Path.</param>
        [Export("collectionView:didSelectItemAtIndexPath:")]
        protected void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var sidebarItem = this.dataSource.GetItemIdentifier(indexPath)!;
            sidebarItem!.Item.IsRead = true;
            this.SelectedItem = sidebarItem!.Item;
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

        private class FeedListCell : UICollectionViewListCell
        {
            private FeedItem? item;
            private UILabel titleLabel = new UILabel() { Font = UIFont.PreferredHeadline, Lines = 3, LineBreakMode = UILineBreakMode.TailTruncation };
            private UILabel descriptionLabel = new UILabel() { Font = UIFont.PreferredCaption1, Lines = 2, LineBreakMode = UILineBreakMode.TailTruncation };
            private UIView contentHolder = new UIView();
            private UIView hasSeenHolder = new UIView();
            private UIImageView hasSeenIcon = new UIImageView();
            private UIImageView feedImageIcon = new UIImageView();

            protected internal FeedListCell(NativeHandle handle)
                : base(handle)
            {
                this.ContentView.AutoSetDimension(ALDimension.Height, 85f);
                this.ContentView.AddSubview(this.hasSeenHolder);
                this.ContentView.AddSubview(this.feedImageIcon);
                this.hasSeenHolder.AddSubview(this.hasSeenIcon);

                this.hasSeenHolder.AutoPinEdgesToSuperviewEdgesExcludingEdge(UIEdgeInsets.Zero, ALEdge.Right);
                this.hasSeenHolder.AutoSetDimension(ALDimension.Width, 25f);

                this.hasSeenIcon.AutoCenterInSuperview();
                this.hasSeenIcon.AutoSetDimensionsToSize(new CGSize(12, 12));

                this.feedImageIcon.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 5f);
                this.feedImageIcon.AutoPinEdge(ALEdge.Left, ALEdge.Right, this.hasSeenHolder, 5f);
                this.feedImageIcon.AutoSetDimensionsToSize(new CGSize(50f, 50f));
            }

            public void SetupCell(FeedItem item)
            {
                this.item = item;
                this.feedImageIcon.Image = UIImage.LoadFromData(NSData.FromArray(item.Feed!.ImageCache!))!.Scale(new CGSize(50f, 50f), 2f).WithRoundedCorners(5f);
                this.titleLabel.Text = item.Title;
                var htmlString = !string.IsNullOrEmpty(item.Description) ? item.Description : item.Content;

                // We don't want to render the HTML, we just want to get the raw text out.
                this.descriptionLabel.Text = Regex.Replace(htmlString ?? string.Empty, "<[^>]*>", string.Empty)!.Trim();

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