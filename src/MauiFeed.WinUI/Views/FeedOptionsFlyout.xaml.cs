// <copyright file="FeedOptionsFlyout.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Drastic.Tools;
using Microsoft.UI.Xaml.Controls;

namespace MauiFeed.WinUI.Views
{
    public sealed partial class FeedOptionsFlyout : UserControl
    {
        public FeedOptionsFlyout(AsyncCommand<FeedNavigationViewItem> removeFeedListItemCommand, FeedNavigationViewItem item)
        {
            this.FeedListItem = item;
            this.InitializeComponent();
            this.RemoveFeedListItemCommand = removeFeedListItemCommand;
        }

        public AsyncCommand<FeedNavigationViewItem> RemoveFeedListItemCommand { get; }

        public FeedNavigationViewItem FeedListItem { get; set; }
    }
}
