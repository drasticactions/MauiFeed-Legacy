// <copyright file="SidebarListCell.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using MauiFeed.MacCatalyst.Views;
using ObjCRuntime;
using static System.Net.Mime.MediaTypeNames;

namespace MauiFeed.MacCatalyst.Sidebar
{
    /// <summary>
    /// Sidebar List Cell.
    /// </summary>
    public class SidebarListCell : UICollectionViewListCell
    {
        private SidebarItem? item;
        private PaddingLabel unreadLabel;
        private UIListContentConfiguration config;

        /// <summary>
        /// Initializes a new instance of the <see cref="SidebarListCell"/> class.
        /// </summary>
        /// <param name="handle">Handle Generator.</param>
        protected internal SidebarListCell(NativeHandle handle)
          : base(handle)
        {
            this.unreadLabel = new PaddingLabel() { };
            this.unreadLabel.TextEdgeInsets = new UIEdgeInsets(2, 5, 2, 5);
            this.ContentConfiguration = this.config = UIListContentConfiguration.SidebarSubtitleCellConfiguration;
        }

        /// <summary>
        /// Gets the sidebar item.
        /// </summary>
        public SidebarItem? Item => this.item;

        /// <summary>
        /// Setup a cell with a given sidebar item.
        /// </summary>
        /// <param name="item">Sidebar Item.</param>
        public void SetupCell(SidebarItem item)
        {
            this.item = item;
            this.item.Cell = this;
            this.config.Text = this.item.Title;
            this.config.Image = this.item.Image;
            this.config.TextProperties.Font = UIFont.PreferredSubheadline;
            this.config.TextProperties.Color = UIColor.SecondaryLabel;
            this.UpdateIsRead();
            this.ContentConfiguration = this.config;
        }

        /// <summary>
        /// Update IsRead count.
        /// </summary>
        public void UpdateIsRead()
        {
            var count = this.item?.UnreadCount;

            this.InvokeOnMainThread(() =>
            {
                if (count > 0)
                {
                    this.unreadLabel.Text = count.ToString();
                    var test2 = new UICellAccessoryCustomView(this.unreadLabel, UICellAccessoryPlacement.Trailing);
                    ((UICollectionViewListCell)this).Accessories = this.item?.RowType == SidebarItemRowType.Row ? new UICellAccessory[] { test2 } : new UICellAccessory[] { new UICellAccessoryOutlineDisclosure(), test2 };
                }
                else
                {
                    ((UICollectionViewListCell)this).Accessories = this.item?.RowType == SidebarItemRowType.Row ? new UICellAccessory[] { } : new UICellAccessory[] { new UICellAccessoryOutlineDisclosure() };
                }
            });
        }
    }
}