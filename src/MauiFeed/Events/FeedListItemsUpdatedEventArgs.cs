using MauiFeed.Models;

namespace MauiFeed.Events
{
    public class FeedListItemsUpdatedEventArgs : EventArgs
    {
        public IEnumerable<FeedListItem> FeedListItems;

        public FeedListItemsUpdatedEventArgs(IList<FeedListItem> items)
        {
            this.FeedListItems = items;
        }

        public FeedListItemsUpdatedEventArgs(FeedItem item)
        {
            this.FeedListItems = new List<FeedListItem>() { item.Feed! };
        }

        public FeedListItemsUpdatedEventArgs(FeedListItem item)
        {
            this.FeedListItems = new List<FeedListItem>() { item };
        }

        public FeedListItemsUpdatedEventArgs(IList<FeedItem> items)
        {
            this.FeedListItems = items.Select(n => n.Feed!).DistinctBy(n => n.Id);
        }
    }
}
