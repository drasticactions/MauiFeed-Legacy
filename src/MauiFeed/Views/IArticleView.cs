using System;
using MauiFeed.Models;

namespace MauiFeed.Views
{
	public interface IArticleView
	{
		void SetFeedItem(FeedItem item);
	}
}