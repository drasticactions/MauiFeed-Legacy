// <copyright file="EFCoreDatabaseContext.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Events;
using MauiFeed.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections;
using System.Linq.Expressions;

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

        /// <summary>
        /// Fired when a feed item updates.
        /// </summary>
        public event EventHandler<EventArgs>? OnDatabaseUpdated;

        public DbSet<FeedListItem>? FeedListItems { get; set; }

        public DbSet<FeedItem>? FeedItems { get; set; }

        public int GetUnreadCountForFeedListItem(FeedListItem item)
            => this.FeedItems!.Count(n => n.FeedListItemId == item.Id && !n.IsRead);

        public async Task<int> UpdateFeedListItems(IList<FeedListItem> feedListItems)
        {
            this.FeedListItems!.UpdateRange(feedListItems);
            var result = await this.SaveChangesAsync();
            this.OnDatabaseUpdated?.Invoke(this, EventArgs.Empty);
            return result;
        }

        public async Task<int> UpdateFeedItems(IList<FeedItem> feedItems)
        {
            this.FeedItems!.UpdateRange(feedItems);
            var result = await this.SaveChangesAsync();
            this.OnDatabaseUpdated?.Invoke(this, EventArgs.Empty);
            return result;
        }

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
            this.OnDatabaseUpdated?.Invoke(this, EventArgs.Empty);
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
            this.OnDatabaseUpdated?.Invoke(this, EventArgs.Empty);
            return item;
        }

        public async Task<FeedListItem> UpdateFeedListItem(FeedListItem feedListItem)
        {
            await this.FeedListItems!.Upsert(feedListItem).On(n => new { n.Uri }).RunAsync();
            await this.SaveChangesAsync();
            this.OnFeedListItemUpdated?.Invoke(this, new FeedListItemUpdatedEventArgs(feedListItem));
            this.OnDatabaseUpdated?.Invoke(this, EventArgs.Empty);
            return feedListItem;
        }

        public async Task<FeedItem> UpdateFeedItem(FeedItem item)
        {
            await this.FeedItems!.Upsert(item).On(n => new { n.RssId }).RunAsync();
            await this.SaveChangesAsync();
            this.OnFeedItemUpdated?.Invoke(this, new FeedItemUpdatedEventArgs(item));
            this.OnDatabaseUpdated?.Invoke(this, EventArgs.Empty);
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

        public Task<List<FeedListItem>> GetAllFeedListAsync()
        {
            throw new NotImplementedException();
        }

        public Expression<Func<TData, bool>> CreateFilter<TData, TKey>(Expression<Func<TData, TKey>> selector, TKey valueToCompare, FilterType type)
        {
            var parameter = Expression.Parameter(typeof(TData));
            var expressionParameter = Expression.Property(parameter, GetParameterName(selector));

            Expression? body = null;

            switch (type)
            {
                case FilterType.Equals:
                    body = Expression.Equal(expressionParameter, Expression.Constant(valueToCompare, typeof(TKey)));
                    break;
                case FilterType.GreaterThan:
                    body = Expression.GreaterThan(expressionParameter, Expression.Constant(valueToCompare, typeof(TKey)));
                    break;
                case FilterType.GreaterThanOrEqual:
                    body = Expression.GreaterThanOrEqual(expressionParameter, Expression.Constant(valueToCompare, typeof(TKey)));
                    break;
                case FilterType.LessThan:
                    body = Expression.LessThan(expressionParameter, Expression.Constant(valueToCompare, typeof(TKey)));
                    break;
                case FilterType.LessThanOrEqual:
                    body = Expression.LessThanOrEqual(expressionParameter, Expression.Constant(valueToCompare, typeof(TKey)));
                    break;
                default:
                    throw new ArgumentNullException(nameof(type));
            }
            return Expression.Lambda<Func<TData, bool>>(body, parameter);
        }

        private string GetParameterName<TData, TKey>(Expression<Func<TData, TKey>> expression)
        {

            if (expression.Body is UnaryExpression x)
            {
                var type = x.Operand.GetType();
                var mem = x.Operand;
            }

            if (!(expression.Body is MemberExpression memberExpression))
            {
                memberExpression = (MemberExpression)((UnaryExpression)expression.Body).Operand;
            }

            return memberExpression.ToString().Substring(2);
        }

        public enum FilterType
        {
            Equals,
            GreaterThan,
            LessThan,
            GreaterThanOrEqual,
            LessThanOrEqual,
        }
    }
}
