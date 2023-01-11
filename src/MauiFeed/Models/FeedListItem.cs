// <copyright file="FeedListItem.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MauiFeed.Models
{
    /// <summary>
    /// Feed List Item.
    /// </summary>
    public class FeedListItem : INotifyPropertyChanged
    {
        private bool isFavorite;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedListItem"/> class.
        /// </summary>
        public FeedListItem()
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
        /// Gets or sets the folder id, optional.
        /// </summary>
        public int? FolderId { get; set; }

        /// <summary>
        /// Gets or sets the feed name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the feed description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the feed Language.
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Gets or sets the last updated date.
        /// </summary>
        public DateTime? LastUpdatedDate { get; set; }

        /// <summary>
        /// Gets or sets the last updated date string.
        /// </summary>
        public string? LastUpdatedDateString { get; set; }

        /// <summary>
        /// Gets or sets the image uri.
        /// </summary>
        public Uri? ImageUri { get; set; }

        /// <summary>
        /// Gets or sets the Feed Uri.
        /// </summary>
        public Uri? Uri { get; set; }

        /// <summary>
        /// Gets or sets the image cache.
        /// </summary>
        public byte[]? ImageCache { get; set; }

        /// <summary>
        /// Gets or sets the Feed Link.
        /// </summary>
        public string? Link { get; set; }

        /// <summary>
        /// Gets or sets the type of FeedListItem this is.
        /// This helps organize where in the list it goes.
        /// </summary>
        public FeedListItemType Type { get; set; }

        public virtual IEnumerable<FeedItem>? Items { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the feed is favorited.
        /// </summary>
        public bool IsFavorite
        {
            get { return this.isFavorite; }
            set { this.SetProperty(ref this.isFavorite, value); }
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
