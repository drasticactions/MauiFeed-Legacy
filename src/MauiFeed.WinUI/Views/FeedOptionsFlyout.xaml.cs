// <copyright file="FeedOptionsFlyout.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Drastic.Tools;
using MauiFeed.Models;
using MauiFeed.Services;
using MauiFeed.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace MauiFeed.WinUI.Views
{
    public sealed partial class FeedOptionsFlyout : UserControl
    {
        private ISidebarView view;

        public FeedOptionsFlyout(ISidebarView view, AsyncCommand<FeedNavigationViewItem> removeFeedListItemCommand, AsyncCommand<FeedNavigationViewItem> removeFromFolderCommand, FeedNavigationViewItem item)
        {
            this.view = view;
            this.FeedListItem = item;
            this.InitializeComponent();
            this.RemoveFeedListItemCommand = removeFeedListItemCommand;
            this.RemoveFromFolderCommand = removeFromFolderCommand;
            this.DataContext = this;
        }

        public AsyncCommand<FeedNavigationViewItem> RemoveFeedListItemCommand { get; }

        public AsyncCommand<FeedNavigationViewItem> RemoveFromFolderCommand { get; }

        public FeedNavigationViewItem FeedListItem { get; set; }

        public void SetFolders(List<FeedNavigationViewItem> folders)
        {
            this.AddFolderMenuFlyout.Items.Clear();

            foreach (var item in folders)
            {
                var folder = new FolderMenuFlyoutItem(item);
                folder.Click += Folder_Click;
                this.AddFolderMenuFlyout.Items.Add(folder);
            }
        }

        private void Folder_Click(object sender, RoutedEventArgs e)
        {
            var folder = (FolderMenuFlyoutItem)sender!;
            this.view.MoveItemToFolder(this.FeedListItem, folder.Folder);
        }

        private class FolderMenuFlyoutItem : MenuFlyoutItem
        {
            public FolderMenuFlyoutItem(FeedNavigationViewItem folder)
            {
                this.Folder = folder;
                this.Text = folder.Folder!.Name;
            }

            public FeedNavigationViewItem Folder { get; }
        }
    }
}
