// <copyright file="ISidebarView.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;

namespace MauiFeed.Views
{
    /// <summary>
    /// Sidebar View.
    /// </summary>
    public interface ISidebarView
    {
        /// <summary>
        /// Update the sidebar UI.
        /// </summary>
        void UpdateSidebar();

        /// <summary>
        /// Add an item to the sidebar.
        /// </summary>
        /// <param name="item">Feed List Item.</param>
        void AddItemToSidebar(FeedListItem item);

        /// <summary>
        /// Generate Sidebar Items.
        /// </summary>
        void GenerateSidebar();

        void MoveItemToFolder(ISidebarItem item, ISidebarItem folder);

        void RemoveFromFolder(ISidebarItem item, bool moveToRoot = false);
    }
}