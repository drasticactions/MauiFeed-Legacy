// <copyright file="FolderOptionsFlyout.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MauiFeed.Views;
using Drastic.Tools;
using MauiFeed.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MauiFeed.WinUI.Views
{
    public sealed partial class FolderOptionsFlyout : UserControl
    {

        private AsyncCommand<FeedNavigationViewItem> command;
        private AsyncCommand<FeedNavigationViewItem> removeCommand;

        public FolderOptionsFlyout(AsyncCommand<FeedNavigationViewItem> command, AsyncCommand<FeedNavigationViewItem> removeCommand, FeedNavigationViewItem item, FeedFolder? folder = default)
        {
            this.command = command;
            this.removeCommand = removeCommand;
            this.Item = item;
            this.Folder = folder ?? new FeedFolder();
            this.InitializeComponent();
        }

        public FeedFolder Folder { get; set; }

        public FeedNavigationViewItem Item { get; set; }

        public AsyncCommand<FeedNavigationViewItem> AddOrUpdateFeedFolder => this.command;

        public AsyncCommand<FeedNavigationViewItem> RemoveFeedFolder => this.removeCommand;

        public bool IsExistingItem => this.Folder?.Id > 0;

        private async void FeedUrlField_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (string.IsNullOrEmpty(this.FeedUrlField.Text))
                {
                    return;
                }

                this.Folder.Name = this.FeedUrlField.Text;
                await this.command.ExecuteAsync(this.Item);
            }
        }
    }
}
