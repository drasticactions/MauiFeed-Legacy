// <copyright file="FeedListViewModel.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using Drastic.Tools;
using MauiFeed.Events;
using MauiFeed.Models;

namespace MauiFeed.ViewModels
{
    public class FeedListViewModel : MainFeedViewModel
    {
        public FeedListViewModel(IServiceProvider services)
            : base(services)
        {
            this.FeedListItems = new ObservableCollection<FeedListItem>();
            this.FeedListItemSelectedCommand = new AsyncCommand<FeedListItem>(
            async (item) => this.OnFeedListItemSelected?.Invoke(this, new FeedListItemSelectedEventArgs(item)),
            null,
            this.ErrorHandler);
        }

        /// <summary>
        /// Fired when a feed list item updates.
        /// </summary>
        public event EventHandler<FeedListItemSelectedEventArgs>? OnFeedListItemSelected;

        /// <summary>
        /// Gets the UpdateFeedListItem.
        /// </summary>
        public AsyncCommand<FeedListItem> FeedListItemSelectedCommand { get; private set; }

        /// <summary>
        /// Gets the list of feed list items.
        /// </summary>
        public ObservableCollection<FeedListItem> FeedListItems { get; }

        /// <inheritdoc/>
        public override async Task OnLoad()
        {
            await base.OnLoad();
            this.UpdateFeeds().FireAndForgetSafeAsync(this.ErrorHandler);
        }

        private async Task UpdateFeeds()
        {
            var feedList = await this.Context.GetAllFeedListAsync();
            foreach (var newItem in feedList)
            {
                var item = this.FeedListItems.FirstOrDefault(n => n.Uri == newItem.Uri);
                if (item is not null)
                {
                    this.FeedListItems[this.FeedListItems.IndexOf(item)] = newItem;
                }
                else
                {
                    this.FeedListItems.Add(newItem);
                }
            }
        }
    }
}
