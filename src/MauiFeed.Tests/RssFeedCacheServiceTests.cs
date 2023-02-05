// <copyright file="RssFeedCacheServiceTests.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Reflection;
using MauiFeed.NewsService;
using MauiFeed.Services;
using Microsoft.EntityFrameworkCore;

namespace MauiFeed.Tests
{
    /// <summary>
    /// RssFeedCacheService Test.
    /// </summary>
    [TestClass]
    public class RssFeedCacheServiceTests
    {
        /// <summary>
        /// Test Cache Handling.
        /// </summary>
        /// <returns>Task.</returns>
        [TestMethod]
        public async Task HandleFeeds()
        {
            var dbFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly()!.Location!)!, "database.db");
            if (File.Exists(dbFile))
            {
                File.Delete(dbFile);
            }

            var errorHandler = new TestErrorHandler();
            var rssService = new FeedReaderService(errorHandler);
            var databaseContext = new DatabaseContext(dbFile);
            var rssCache = new RssFeedCacheService(rssService, databaseContext);

            // First, add the initial feed.
            var first = await rssCache.RetrieveFeedAsync("https://devblogs.microsoft.com/dotnet/feed/");
            var unreadItemsCount = first.Items!.Where(n => !n.IsRead).Count();

            // This should be the first item in the DB. So it should be "1".
            Assert.IsTrue(first.Id == 1);

            var two = await rssCache.RetrieveFeedAsync("https://devblogs.microsoft.com/dotnet/category/maui/feed/");

            // This should be two...
            Assert.IsTrue(two.Id == 2);

            var firstAgain = await rssCache.RetrieveFeedAsync("https://devblogs.microsoft.com/dotnet/feed/");

            // This should be the same as the first.
            Assert.IsTrue(firstAgain.Id == 1);
        }
    }
}
