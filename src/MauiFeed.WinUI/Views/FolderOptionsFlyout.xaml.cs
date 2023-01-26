// <copyright file="FolderOptionsFlyout.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Services;
using Drastic.Tools;
using MauiFeed.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace MauiFeed.WinUI.Views
{
    /// <summary>
    /// Folder Options Flyout.
    /// </summary>
    public sealed partial class FolderOptionsFlyout : UserControl
    {
        private MainWindow sidebar;
        private AsyncCommand<FeedSidebarItem> command;
        private AsyncCommand<FeedSidebarItem> removeCommand;
        private IErrorHandlerService errorHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderOptionsFlyout"/> class.
        /// </summary>
        /// <param name="sidebar">Main Window Sidebar.</param>
        /// <param name="item">Feed SIdebar Item.</param>
        /// <param name="folder">Folder.</param>
        public FolderOptionsFlyout(MainWindow sidebar, FeedSidebarItem item)
        {
            this.InitializeComponent();
            this.sidebar = sidebar;
            this.Item = item;
            this.Folder = item.FeedFolder;
            this.errorHandler = Ioc.Default.GetService<IErrorHandlerService>()!;
            this.command = new AsyncCommand<FeedSidebarItem>(this.UpdateFolder, null, this.errorHandler);
            this.removeCommand = new AsyncCommand<FeedSidebarItem>(this.RemoveFolder, null, this.errorHandler);
        }

        /// <summary>
        /// Gets the Add Or Update Feed Folder Command.
        /// </summary>
        public AsyncCommand<FeedSidebarItem> AddOrUpdateFeedFolder => this.command;

        /// <summary>
        /// Gets the Remote Feed Folder Command.
        /// </summary>
        public AsyncCommand<FeedSidebarItem> RemoveFeedFolder => this.removeCommand;

        /// <summary>
        /// Gets or sets the feed folder.
        /// </summary>
        public FeedFolder? Folder { get; set; }

        /// <summary>
        /// Gets a value indicating whether the folder already exists.
        /// </summary>
        public bool IsExistingItem => this.Folder?.Id > 0;

        /// <summary>
        /// Gets or sets the feed sidebar item.
        /// </summary>
        public FeedSidebarItem Item { get; set; }

        private Task UpdateFolder(FeedSidebarItem item)
        {
            return Task.CompletedTask;
        }

        private Task RemoveFolder(FeedSidebarItem item)
        {
            return Task.CompletedTask;
        }

        private async void FeedUrlField_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (string.IsNullOrEmpty(this.FeedUrlField.Text))
                {
                    return;
                }

                this.Folder!.Name = this.FeedUrlField.Text;
                await this.command.ExecuteAsync(this.Item);
            }
        }
    }
}
