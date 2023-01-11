// <copyright file="SearchFeedNavigationViewItem.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Linq.Expressions;
using MauiFeed.Models;
using MauiFeed.Services;
using Microsoft.UI.Xaml.Controls;

namespace MauiFeed.WinUI.Views
{
    public class SearchFeedNavigationViewItem : FeedNavigationViewItem
    {
        private string searchTerm;

        public SearchFeedNavigationViewItem(string searchTerm, IconElement icon, DatabaseContext context, Expression<Func<FeedItem, bool>>? filter = null)
            : base(searchTerm, icon, context, filter)
        {
            this.searchTerm = searchTerm;
        }

        public override IList<FeedItem> Items
        {
            get
            {
                return this.context.FeedItems!.Where(n => (n.Content ?? string.Empty).Contains(this.searchTerm)).OrderByDescending(n => n.PublishingDate).ToList();
            }
        }
    }
}
