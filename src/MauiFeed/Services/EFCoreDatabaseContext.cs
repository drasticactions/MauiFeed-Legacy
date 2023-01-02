// <copyright file="EFCoreDatabaseContext.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Events;
using MauiFeed.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace MauiFeed.Services
{
    public class EFCoreDatabaseContext : DbContext, IDatabaseService
    {
        private string databasePath = "database.db";

        public EFCoreDatabaseContext(string databasePath = "")
        {
            if (!string.IsNullOrEmpty(databasePath))
            {
                this.databasePath = databasePath;
            }

            this.Database.EnsureCreated();
        }

        /// <summary>
        /// Fired when a feed item updates.
        /// </summary>
        public event EventHandler<FeedItemUpdatedEventArgs>? OnFeedItemUpdated;

        /// <summary>
        /// Fired when a feed item updates.
        /// </summary>
        public event EventHandler<FeedListItemUpdatedEventArgs>? OnFeedListItemUpdated;

        public DbSet<FeedListItem>? FeedListItems { get; set; }

        public DbSet<FeedItem>? FeedItems { get; set; }

        public int GetUnreadCountForFeedListItem(FeedListItem item)
            => this.FeedItems!.Count(n => n.FeedListItemId == item.Id && !n.IsRead);

        public async Task<FeedListItem> AddOrUpdateFeedListItem(FeedListItem feedListItem)
        {
            var ogItem = await this.FeedListItems!.AsNoTracking().FirstOrDefaultAsync(n => n.Uri == feedListItem.Uri);
            if (ogItem != null)
            {
                feedListItem.Id = ogItem.Id;
                await this.FeedListItems!.Upsert(feedListItem).On(n => new { n.Uri }).RunAsync();
            }
            else
            {
                await this.FeedListItems!.AddAsync(feedListItem);
            }

            await this.SaveChangesAsync();
            this.OnFeedListItemUpdated?.Invoke(this, new FeedListItemUpdatedEventArgs(feedListItem));
            return feedListItem;
        }

        public async Task<FeedItem> AddOrUpdateFeedItem(FeedItem item)
        {
            var ogItem = await this.FeedItems!.AsNoTracking().FirstOrDefaultAsync(n => n.RssId == item.RssId);
            if (ogItem != null)
            {
                item.Id = ogItem.Id;
                await this.FeedItems!.Upsert(item).On(n => new { n.RssId }).RunAsync();
            }
            else
            {
                await this.FeedItems!.AddAsync(item);
            }

            await this.SaveChangesAsync();
            this.OnFeedItemUpdated?.Invoke(this, new FeedItemUpdatedEventArgs(item));
            return item;
        }

        public async Task<FeedListItem> UpdateFeedListItem(FeedListItem feedListItem)
        {
            await this.FeedListItems!.Upsert(feedListItem).On(n => new { n.Uri }).RunAsync();
            await this.SaveChangesAsync();
            this.OnFeedListItemUpdated?.Invoke(this, new FeedListItemUpdatedEventArgs(feedListItem));
            return feedListItem;
        }

        public async Task<FeedItem> UpdateFeedItem(FeedItem item)
        {
            await this.FeedItems!.Upsert(item).On(n => new { n.RssId }).RunAsync();
            await this.SaveChangesAsync();
            this.OnFeedItemUpdated?.Invoke(this, new FeedItemUpdatedEventArgs(item));
            return item;
        }

        /// <summary>
        /// Resets the local database.
        /// </summary>
        public void ResetDatabase()
        {
            this.GetDependencies().StateManager.ResetState();
            this.Database.EnsureDeleted();
            this.Database.EnsureCreated();
        }

        /// <summary>
        /// Run when configuring the database.
        /// </summary>
        /// <param name="optionsBuilder"><see cref="DbContextOptionsBuilder"/>.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={this.databasePath}");
            optionsBuilder.EnableSensitiveDataLogging();
        }

        /// <summary>
        /// Run when building the model.
        /// </summary>
        /// <param name="modelBuilder"><see cref="ModelBuilder"/>.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<FeedFolder>().HasKey(n => n.Id);
            modelBuilder.Entity<FeedListItem>().HasKey(n => n.Id);
            modelBuilder.Entity<FeedListItem>().HasIndex(n => n.Uri).IsUnique();
            modelBuilder.Entity<FeedItem>().HasKey(n => n.Id);
            modelBuilder.Entity<FeedItem>().HasIndex(n => n.RssId).IsUnique();
        }
    }
}
