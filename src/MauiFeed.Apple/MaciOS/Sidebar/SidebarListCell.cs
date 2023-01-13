// <copyright file="SidebarListCell.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using ObjCRuntime;

namespace MauiFeed.Apple
{
    public class SidebarListCell : UICollectionViewListCell
    {
        private SidebarItem? item;
        private PaddingLabel unreadLabel;
        private UIListContentConfiguration config;

        protected internal SidebarListCell(NativeHandle handle)
           : base(handle)
        {
            this.unreadLabel = new PaddingLabel() { };
            this.unreadLabel.TextEdgeInsets = new UIEdgeInsets(2, 5, 2, 5);
#if TVOS
            this.ContentConfiguration = this.config = UIListContentConfiguration.SubtitleCellConfiguration;
#else
            this.ContentConfiguration = this.config = UIListContentConfiguration.SidebarSubtitleCellConfiguration;
#endif
        }

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

        public void UpdateIsRead()
        {
            var count = this.item?.UnreadCount;

            this.InvokeOnMainThread(() =>
            {
                if (count > 0)
                {
                    this.unreadLabel.Text = count.ToString();
                    var test2 = new UICellAccessoryCustomView(this.unreadLabel, UICellAccessoryPlacement.Trailing);
                    ((UICollectionViewListCell)this).Accessories = new UICellAccessory[] { test2 };
                }
                else
                {
                    ((UICollectionViewListCell)this).Accessories = new UICellAccessory[] { };
                }
            });
        }

        private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.UpdateIsRead();
        }
    }
}