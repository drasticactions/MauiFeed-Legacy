using System;
using MauiFeed.Models;

namespace MauiFeed.Apple
{
    public class TimelineCollectionViewController : UIViewController, Views.ITimelineView
    {
        public TimelineCollectionViewController()
        {
        }

        public void SetFeedItems(IList<FeedItem> feedItems)
        {
            throw new NotImplementedException();
        }
    }
}