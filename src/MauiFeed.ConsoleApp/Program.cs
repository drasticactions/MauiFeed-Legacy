// <copyright file="Program.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using MauiFeed.NewsService;
using MauiFeed.Services;
using Sharprompt;

var efcoredatabase = new DatabaseContext();
var feedReader = new FeedReaderService();
var cache = new RssFeedCacheService(feedReader, efcoredatabase);

var runApp = true;

while (runApp)
{
    Console.WriteLine("MauiFeed DB creator 2000");
    var value = Prompt.Select<MainMenu>("Main Menu");
    switch (value)
    {
        case MainMenu.AddFeed:
            await AddRssFeed();
            break;
        case MainMenu.ListFeeds:
            await ListFeeds();
            break;
        case MainMenu.RemoveFeed:
            break;
        case MainMenu.RefreshFeeds:
            await RefreshFeeds();
            break;
        default:
        case MainMenu.Exit:
            runApp = false;
            break;
    }

    Console.Clear();
}

async Task ListFeeds()
{
    try
    {
        var feeds = await efcoredatabase!.GetAllFeedListAsync();
        for (int i = 0; i < feeds.Count; i++)
        {
            MauiFeed.Models.FeedListItem? feed = feeds[i];
            Console.WriteLine($"{i + 1}: {feed.Name}");
            Console.WriteLine($"Total Items - {feed.Items!.Count()}");
        }

        Console.ReadLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error:");
        Console.WriteLine(ex);
        Console.ReadLine();
    }
}

async Task RefreshFeeds()
{
    try
    {
        var progress = new Progress<RssCacheFeedUpdate>();
        progress.ProgressChanged += Progress_ProgressChanged;
        await cache!.RefreshFeedsAsync(progress);
        progress.ProgressChanged -= Progress_ProgressChanged;
        Console.ReadLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error:");
        Console.WriteLine(ex);
        Console.ReadLine();
    }
}

void Progress_ProgressChanged(object? sender, RssCacheFeedUpdate e)
{
    Console.WriteLine($"{e.FeedsCompleted}/{e.TotalFeeds}: Is Done - {e.IsDone}");
}

async Task AddRssFeed()
{
    var rssFeedUrl = Prompt.Input<string>("Enter RSS Feed");
    try
    {
        var result = await cache.RetrieveFeedAsync(new Uri(rssFeedUrl));
        Console.WriteLine(result.Name);
        Console.WriteLine($"Total: {result.Items!.Count()}");
        Console.ReadLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error:");
        Console.WriteLine(ex);
        Console.ReadLine();
    }
}

enum MainMenu
{
    [Display(Name = "List Feeds")]
    ListFeeds,

    [Display(Name = "Add Feed")]
    AddFeed,

    [Display(Name = "Remove Feed")]
    RemoveFeed,

    [Display(Name = "Refresh Feed")]
    RefreshFeeds,

    [Display(Name = "Exit")]
    Exit,
}