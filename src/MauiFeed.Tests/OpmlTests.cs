// <copyright file="OpmlTests.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Xml;
using MauiFeed.Models.OPML;
using MauiFeed.Services;

namespace MauiFeed.Tests
{
    /// <summary>
    /// Opml Tests.
    /// </summary>
    [TestClass]
    public class OpmlTests
    {
        private DatabaseContext db;
        private OpmlFeedListItemFactory opmlFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpmlTests"/> class.
        /// </summary>
        public OpmlTests()
        {
            if (File.Exists("db-opml.db"))
            {
                File.Delete("db-opml.db");
            }

            this.db = new DatabaseContext("db-opml.db");
            this.opmlFactory = new OpmlFeedListItemFactory(this.db);
        }

        /// <summary>
        /// Parse Opml.
        /// </summary>
        /// <returns>Task.</returns>
        [TestMethod]
        public async Task ParseOpml()
        {
            var text = File.ReadAllText("Data/Subscriptions.opml");
            var xml = new XmlDocument();
            xml.LoadXml(text);
            var opml = new Opml(xml);
            Assert.IsNotNull(opml);

            var result = await this.opmlFactory.GenerateFeedListItemsFromOpmlAsync(opml);
            Assert.IsTrue(result > 0);

            // Running the same list twice should result in no items changed.
            result = await this.opmlFactory.GenerateFeedListItemsFromOpmlAsync(opml);
            Assert.IsTrue(result == 0);
        }
    }
}
