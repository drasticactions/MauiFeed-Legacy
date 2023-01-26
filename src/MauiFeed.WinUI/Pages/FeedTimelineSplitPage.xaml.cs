// <copyright file="FeedTimelineSplitPage.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiFeed.Models;
using MauiFeed.WinUI.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MauiFeed.WinUI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FeedTimelineSplitPage : Page, INotifyPropertyChanged
    {
        private FeedSidebarItem? selectedSidebarItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedTimelineSplitPage"/> class.
        /// </summary>
        public FeedTimelineSplitPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.ArticleList.DataContext = this;
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the list of items.
        /// </summary>
        public ObservableCollection<FeedItem> Items { get; private set; } = new ObservableCollection<FeedItem>();

        /// <summary>
        /// Gets or sets the element theme.
        /// </summary>
        public FeedSidebarItem? SelectedSidebarItem
        {
            get
            {
                return this.selectedSidebarItem;
            }

            set
            {
                this.SetProperty(ref this.selectedSidebarItem, value);
                this.UpdateFeed();
            }
        }

        /// <summary>
        /// Gets a value indicating whether to show the icon.
        /// </summary>
        public bool ShowIcon
        {
            get
            {
                // If it's a smart filter or folder, always show the icon.
                if (this.selectedSidebarItem?.ItemType != SidebarItemType.FeedListItem)
                {
                    return true;
                }

                var feed = this.Items.Select(n => n.Feed).Distinct();
                return feed.Count() > 1;
            }
        }

        /// <summary>
        /// Update the selected feed item, if not null.
        /// </summary>
        public void UpdateFeed()
        {
            this.Items.Clear();

            var items = this.selectedSidebarItem?.Items ?? new List<FeedItem>();

            foreach (var item in items)
            {
                this.Items.Add(item);
            }

            this.OnPropertyChanged(nameof(this.ShowIcon));
        }

        /// <summary>
        /// On Property Changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = this.PropertyChanged;
            if (changed == null)
            {
                return;
            }

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

#pragma warning disable SA1600 // Elements should be documented
        private bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null)
#pragma warning restore SA1600 // Elements should be documented
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                return false;
            }

            backingStore = value;
            onChanged?.Invoke();
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
