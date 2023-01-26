// <copyright file="FeedFolder.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;

namespace MauiFeed.Models
{
    /// <summary>
    /// Feed Folder.
    /// </summary>
    public class FeedFolder
    {
        /// <summary>
        /// Gets or sets the Id of the folder.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the folder.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the list of Feed List Items.
        /// </summary>
        public virtual IEnumerable<FeedListItem>? Items { get; set; }
    }
}
