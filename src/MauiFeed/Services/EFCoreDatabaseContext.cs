// <copyright file="EFCoreDatabaseContext.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using HandlebarsDotNet;
using MauiFeed.Events;
using MauiFeed.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections;
using System.Linq.Expressions;
using static System.Environment;

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
        public event EventHandler<FeedListItemsUpdatedEventArgs>? OnFeedListItemsUpdated;

        public DbSet<FeedListItem>? FeedListItems { get; set; }

        public DbSet<FeedItem>? FeedItems { get; set; }

        public int GetUnreadCountForFeedListItem(FeedListItem item)
            => this.FeedItems!.Count(n => n.FeedListItemId == item.Id && !n.IsRead);

        public async Task<int> UpdateFeedListItems(IList<FeedListItem> feedListItems)
        {
            this.FeedListItems!.UpdateRange(feedListItems);
            var result = await this.SaveChangesAsync();
            this.OnFeedListItemsUpdated?.Invoke(this, new FeedListItemsUpdatedEventArgs(feedListItems));
            return result;
        }

        public async Task<int> UpdateFeedItems(IList<FeedItem> feedItems)
        {
            this.FeedItems!.UpdateRange(feedItems);
            var result = await this.SaveChangesAsync();
            this.OnFeedListItemsUpdated?.Invoke(this, new FeedListItemsUpdatedEventArgs(feedItems));
            return result;
        }

        public async Task<int> AddFeedItems(IList<FeedItem> feedItems)
        {
            await this.FeedItems!.AddRangeAsync(feedItems);
            var result = await this.SaveChangesAsync();
            this.OnFeedListItemsUpdated?.Invoke(this, new FeedListItemsUpdatedEventArgs(feedItems));
            return result;
        }

        public async Task<FeedListItem> AddFeedListItem(FeedListItem feedListItem)
        {
            if (feedListItem.Id > 0)
            {
                throw new ArgumentException("Id Must Be 0");
            }

            await this.FeedListItems!.AddAsync(feedListItem);
            await this.SaveChangesAsync();
            this.OnFeedListItemsUpdated?.Invoke(this, new FeedListItemsUpdatedEventArgs(feedListItem));
            return feedListItem;
        }

        public async Task<FeedItem> AddFeedItem(FeedItem item)
        {
            if (item.Id > 0)
            {
                throw new ArgumentException("Id Must Be 0");
            }

            if (item.FeedListItemId <= 0)
            {
                throw new ArgumentException("FeedListItemId Must Not Be 0");
            }

            await this.FeedItems!.AddAsync(item);
            await this.SaveChangesAsync();
            this.OnFeedListItemsUpdated?.Invoke(this, new FeedListItemsUpdatedEventArgs(item));
            return item;
        }

        public async Task<FeedListItem> UpdateFeedListItem(FeedListItem feedListItem)
        {
            await this.FeedListItems!.Upsert(feedListItem).On(n => new { n.Uri }).RunAsync();
            await this.SaveChangesAsync();
            this.OnFeedListItemsUpdated?.Invoke(this, new FeedListItemsUpdatedEventArgs(feedListItem));
            return feedListItem;
        }

        public async Task<FeedItem> UpdateFeedItem(FeedItem item)
        {
            await this.FeedItems!.Upsert(item).On(n => new { n.RssId }).RunAsync();
            await this.SaveChangesAsync();
            this.OnFeedListItemsUpdated?.Invoke(this, new FeedListItemsUpdatedEventArgs(item));
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

        public async Task<List<FeedListItem>> GetAllFeedListAsync()
        {
            return await this.FeedListItems!.ToListAsync();
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

        public async Task<FeedItem?> GetFeedItemViaRssId(string? rssId)
        {
            if (rssId is null)
                return null;

            return await this.FeedItems!.FirstOrDefaultAsync(n => n.RssId == rssId);
        }

        public async Task<FeedListItem?> GetFeedListItem(Uri? rssId)
        {
            if (rssId is null)
                return null;

            return await this.FeedListItems!.FirstOrDefaultAsync(n => n.Uri == rssId);
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
