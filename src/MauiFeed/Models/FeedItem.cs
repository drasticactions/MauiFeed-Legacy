// <copyright file="FeedItem.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace MauiFeed.Models
{
    /// <summary>
    /// Feed Item.
    /// </summary>
    public class FeedItem : INotifyPropertyChanged
    {
        private bool isRead;
        private bool isFavorite;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedItem"/> class.
        /// </summary>
        public FeedItem()
        {
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the id from the rss feed.
        /// </summary>
        public string? RssId { get; set; }

        /// <summary>
        /// Gets or sets the feed list item id.
        /// </summary>
        public int FeedListItemId { get; set; }

        public virtual FeedListItem? Feed { get; set; }

        /// <summary>
        /// Gets or sets the title of the feed item.
        /// </summary>
        public string? Title
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets link (url) to the feed item.
        /// </summary>
        public string? Link
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets description of the feed item.
        /// </summary>
        public string? Description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets The publishing date as string.
        /// </summary>
        public string? PublishingDateString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets The published date as datetime.
        /// </summary>
        public DateTime? PublishingDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets The author of the feed item.
        /// </summary>
        public string? Author
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets The content of the feed item.
        /// </summary>
        public string? Content
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets The html of the feed item.
        /// </summary>
        public string? Html
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets The image url of the feed item.
        /// </summary>
        public string? ImageUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the feed is favorited.
        /// </summary>
        public bool IsFavorite
        {
            get { return this.isFavorite; }
            set { this.SetProperty(ref this.isFavorite, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the feed item has been read.
        /// </summary>
        public bool IsRead
        {
            get { return this.isRead; }
            set { this.SetProperty(ref this.isRead, value); }
        }

#pragma warning disable SA1600 // Elements should be documented
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null)
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

        /// <summary>
        /// On Property Changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = this.PropertyChanged;
            if (changed == null)
            {
                return;
            }

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
