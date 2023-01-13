// <copyright file="TimelineCollectionViewController.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using MauiFeed.Models;
using MauiFeed.Views;

namespace MauiFeed.Apple
{
    public class TimelineCollectionViewController : UIViewController, IUICollectionViewDelegate, Views.ITimelineView
    {
        private RootSplitViewController controller;

        public TimelineCollectionViewController(RootSplitViewController controller)
        {
            this.controller = controller;
        }

        public Task MarkAllAsRead(List<FeedItem> items)
        {
            return Task.CompletedTask;
        }

        public void SetFeed(ISidebarItem sidebar)
        {
        }

        public void UpdateFeed()
        {
        }
    }
}