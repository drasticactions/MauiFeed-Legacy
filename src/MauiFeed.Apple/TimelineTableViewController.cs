using System;
using MauiFeed.Models;
using MauiFeed.Views;

namespace MauiFeed.Apple
{
    public class TimelineTableViewController : UIViewController, ITimelineView
    {
        private RssTableView table;
        private RootSplitViewController rootSplitViewController;

        public TimelineTableViewController(RootSplitViewController rootSplitViewController)
        {
            this.rootSplitViewController = rootSplitViewController;
            this.View = this.table = new RssTableView(rootSplitViewController);
            this.ViewRespectsSystemMinimumLayoutMargins = false;
            this.View.PreservesSuperviewLayoutMargins = true;
            this.View.DirectionalLayoutMargins = NSDirectionalEdgeInsets.Zero;
        }

        public void SetFeedItems(IList<FeedItem> feedItems)
            => this.table.SetFeedItems(feedItems);

        private class RssTableView : UITableView, ITimelineView
        {
            private RootSplitViewController controller;

            public RssTableView(RootSplitViewController controller)
            {
                this.controller = controller;
                this.Source = new RssTableSource(this.controller);
            }

            public void SetFeedItems(IList<FeedItem> feedItems)
            {
                ((RssTableSource)this.Source).SetFeedItems(feedItems);
                this.ReloadData();
            }
        }

        private class RssTableSource : UITableViewSource, ITimelineView
        {
            private RootSplitViewController controller;
            private IList<FeedItem> items;

            public RssTableSource(RootSplitViewController controller)
            {
                this.controller = controller;
                this.items = new List<FeedItem>();
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return this.items.Count;
            }

            public void SetFeedItems(IList<FeedItem> feedItems)
            {
                this.items = feedItems;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                RssItemViewCell cell = (RssItemViewCell)tableView.DequeueReusableCell(RssItemViewCell.ReuseIdentifier)!;
                FeedItem item = this.items[indexPath.Row];

                if (cell == null)
                {
                    cell = new RssItemViewCell(item);
                }
                else
                {
                    cell.SetupCell(item);
                }

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var item = this.items[indexPath.Row];
                this.controller.FeedWebViewController.SetFeedItem(item);
#if IOS
                this.controller.ShowColumn(UISplitViewControllerColumn.Secondary);
#endif
            }
        }

        private class RssItemViewCell : UITableViewCell
        {
            public static string ReuseIdentifier => "rssItemCell";

            private FeedItem item;

            public RssItemViewCell(FeedItem info, bool showIcon = true, UITableViewCellStyle style = UITableViewCellStyle.Default)
                : base(style, ReuseIdentifier)
            {
                this.item = info;
            }

            public void SetupCell(FeedItem item)
            {
            }
        }
    }
}