using System;
using System.Text.RegularExpressions;
using Drastic.PureLayout;
using Humanizer;
using MauiFeed.Models;

namespace MauiFeed.Apple
{
    public class FeedTableViewController : UIViewController
    {
        private RssTableView table;

        public FeedTableViewController()
        {
            this.View = this.table = new RssTableView();
            this.ViewRespectsSystemMinimumLayoutMargins = false;
            this.View.PreservesSuperviewLayoutMargins = true;
            this.View.DirectionalLayoutMargins = NSDirectionalEdgeInsets.Zero;
        }

        public void Update(FeedItem[] items)
        {
            this.table.Update(items);
        }

        public class RssTableView : UITableView
        {
            private FeedItem[] items;

            public RssTableView()
            {
                this.items = new FeedItem[0];
                this.Source = new TableSource(this.items);
#if !TVOS
                this.SeparatorStyle = UITableViewCellSeparatorStyle.None;
                this.SeparatorColor = UIColor.Clear;
#endif
            }

            public void Update(FeedItem[] items)
            {
                this.items = items;
                this.Source = new TableSource(this.items);
                this.ReloadData();
#if !TVOS
                this.ScrollToRow(NSIndexPath.FromRowSection(0, 0), UITableViewScrollPosition.Top, false);
#endif
            }
        }

        public class TableSource : UITableViewSource
        {
            FeedItem[] TableItems;
            string CellIdentifier = RssItemViewCell.ReuseIdentifier;

            public TableSource(FeedItem[] items)
            {
                TableItems = items;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return TableItems.Length;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                RssItemViewCell cell = (RssItemViewCell)tableView.DequeueReusableCell(CellIdentifier)!;
                FeedItem item = TableItems[indexPath.Row];

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
                var item = this.TableItems[indexPath.Row];
                item.IsRead = true;
                var cell = (RssItemViewCell)tableView.CellAt(indexPath)!;
                cell.UpdateHasSeen(item.IsRead);
            }
        }

        public class RssItemViewCell : UITableViewCell
        {
            public static string ReuseIdentifier => "rssItemCell";

            private FeedItem item;

            private UIView hasSeenHolder = new UIView();
            private UIView iconHolder = new UIView();
            private UIView feedHolder = new UIView();

            private UIView content = new UIView();
            private UIView footer = new UIView();

            private UIImageView hasSeenIcon = new UIImageView();
            private UIImageView icon = new UIImageView();
            private UILabel title = new UILabel() { Lines = 2, Font = UIFont.PreferredHeadline, TextAlignment = UITextAlignment.Left };
            private UILabel description = new UILabel() { Lines = 2, Font = UIFont.PreferredSubheadline, TextAlignment = UITextAlignment.Left };
            private UILabel releaseDate = new UILabel() { Lines = 1, Font = UIFont.PreferredFootnote, TextAlignment = UITextAlignment.Right };
            private UILabel author = new UILabel() { Lines = 1, Font = UIFont.PreferredFootnote, TextAlignment = UITextAlignment.Left };
            private bool showIcon;

            public RssItemViewCell(FeedItem info, bool showIcon = true, UITableViewCellStyle style = UITableViewCellStyle.Default)
          : base(style, ReuseIdentifier)
            {
                this.item = info;
//#if TVOS
//                this.footer.BackgroundColor = UIColor.Clear;
//#else
//                this.footer.BackgroundColor = UIColor.SystemFill;
//#endif
                this.showIcon = showIcon;
                this.icon.Layer.CornerRadius = 5;
                this.icon.Layer.MasksToBounds = true;
                this.SetupUI();
                this.SetupLayout();
                this.SetupCell(info);
            }

            public void SetupUI()
            {
                this.ContentView.AddSubview(this.content);
                this.ContentView.AddSubview(this.footer);

                this.content.AddSubview(this.hasSeenHolder);
                this.content.AddSubview(this.iconHolder);
                this.content.AddSubview(this.feedHolder);

                this.hasSeenHolder.AddSubview(this.hasSeenIcon);

                this.iconHolder.AddSubview(this.icon);

                this.feedHolder.AddSubview(this.title);
                this.feedHolder.AddSubview(this.author);
                this.feedHolder.AddSubview(this.description);
                this.feedHolder.AddSubview(this.releaseDate);

                this.hasSeenIcon.Image = UIImage.GetSystemImage("circle.fill");
            }

            public void SetupLayout()
            {
                this.content.AutoPinEdgesToSuperviewEdgesExcludingEdge(UIEdgeInsets.Zero, ALEdge.Bottom);
                this.content.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, this.footer);
                this.content.AutoSetDimension(ALDimension.Height, 80f);

                this.footer.AutoPinEdgesToSuperviewEdgesExcludingEdge(UIEdgeInsets.Zero, ALEdge.Top);
                this.footer.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, this.content);
                this.footer.AutoSetDimension(ALDimension.Height, 1f);

                this.hasSeenHolder.AutoPinEdgesToSuperviewEdgesExcludingEdge(UIEdgeInsets.Zero, ALEdge.Right);
                this.hasSeenHolder.AutoPinEdge(ALEdge.Right, ALEdge.Left, this.iconHolder);
                this.hasSeenHolder.AutoSetDimension(ALDimension.Width, 25f);

                this.iconHolder.AutoPinEdge(ALEdge.Left, ALEdge.Right, this.hasSeenHolder);
                this.iconHolder.AutoPinEdge(ALEdge.Right, ALEdge.Left, this.feedHolder);
                this.iconHolder.AutoPinEdge(ALEdge.Top, ALEdge.Top, this.content);
                this.iconHolder.AutoPinEdge(ALEdge.Bottom, ALEdge.Bottom, this.content);
                this.iconHolder.AutoSetDimension(ALDimension.Width, 40f);

                this.hasSeenIcon.AutoCenterInSuperview();
                this.hasSeenIcon.AutoSetDimensionsToSize(new CGSize(12, 12));

                this.icon.AutoCenterInSuperview();
                this.icon.AutoSetDimensionsToSize(new CGSize(32, 32));

                this.feedHolder.AutoPinEdgesToSuperviewEdgesExcludingEdge(new UIEdgeInsets(top: 0f, left: 0f, bottom: 0f, right: 0f), ALEdge.Left);
                this.feedHolder.AutoPinEdge(ALEdge.Left, ALEdge.Right, this.iconHolder);

                this.title.AutoPinEdge(ALEdge.Top, ALEdge.Top, this.feedHolder);
                this.title.AutoPinEdge(ALEdge.Right, ALEdge.Right, this.feedHolder, -15f);
                this.title.AutoPinEdge(ALEdge.Left, ALEdge.Left, this.feedHolder, 15f);

                this.description.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, this.title, 0f);
                this.description.AutoPinEdge(ALEdge.Right, ALEdge.Right, this.feedHolder, -15f);
                this.description.AutoPinEdge(ALEdge.Left, ALEdge.Left, this.feedHolder, 15f);

                this.author.AutoPinEdge(ALEdge.Bottom, ALEdge.Bottom, this.feedHolder);
                this.author.AutoPinEdge(ALEdge.Left, ALEdge.Left, this.description);
                this.author.AutoPinEdge(ALEdge.Right, ALEdge.Left, this.releaseDate);

                this.releaseDate.AutoPinEdge(ALEdge.Bottom, ALEdge.Bottom, this.feedHolder);
                this.releaseDate.AutoPinEdge(ALEdge.Right, ALEdge.Right, this.description);
                this.releaseDate.AutoPinEdge(ALEdge.Left, ALEdge.Right, this.author);
            }

            public void SetupCell(FeedItem item)
            {
                this.item = item;
                this.icon.Image = UIImage.LoadFromData(NSData.FromArray(item.Feed!.ImageCache!))!.Scale(new CGSize(32, 32), 2f);
                this.title.Text = item.Title;
                this.description.Text = Regex.Replace(item.Content ?? string.Empty, "<.*?>", string.Empty).Trim();
                this.author.Text = item.Author;
                this.releaseDate.Text = item.PublishingDate.Humanize();

                if (!this.showIcon)
                {
                    this.icon.Hidden = true;
                    this.iconHolder.AutoSetDimension(ALDimension.Width, 0f);
                }

                this.hasSeenIcon.Hidden = item.IsRead;
            }

            public void UpdateHasSeen(bool hasSeen)
                => this.hasSeenIcon.SetHidden(hasSeen, true);
        }
    }
}