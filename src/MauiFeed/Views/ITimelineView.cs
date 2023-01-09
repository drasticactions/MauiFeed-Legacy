using System;
using MauiFeed.Models;

namespace MauiFeed.Views
{
	public interface ITimelineView
	{
		void SetFeedItems(IList<FeedItem> feedItems);
	}
}