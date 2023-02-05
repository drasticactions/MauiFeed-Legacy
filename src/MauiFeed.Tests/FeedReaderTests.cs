// <copyright file="FeedReaderTests.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Drastic.Services;
using MauiFeed.NewsService;
using MauiFeed.Services;

namespace MauiFeed.Tests
{
    /// <summary>
    /// FeedReader Tests.
    /// </summary>
    [TestClass]
    public class FeedReaderTests
    {
        private IRssService rss;
        private IErrorHandlerService errorHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedReaderTests"/> class.
        /// </summary>
        public FeedReaderTests()
        {
            this.errorHandler = new TestErrorHandler();
            this.rss = new FeedReaderService(this.errorHandler);
        }

        /// <summary>
        /// Get Feed.
        /// </summary>
        /// <returns>Task.</returns>
        [TestMethod]
        public async Task GetFeed()
        {
            (var feedList, var feedItemsList) = await this.rss.ReadFeedAsync("https://devblogs.microsoft.com/dotnet/feed/");
            Assert.IsNotNull(feedList);
            Assert.IsNotNull(feedItemsList);
        }
    }
}
