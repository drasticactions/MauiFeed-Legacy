using MauiFeed.Services;

Console.WriteLine("MauiFeed DB creator 2000");

var efcoredatabase = new EFCoreDatabaseContext();
var feedReader = new FeedReaderService();
var cache = new RssFeedCacheService(feedReader, efcoredatabase);

while (true)
{
    Console.WriteLine("Enter RSS Feed:");
    var rssFeedUrl = Console.ReadLine()!;
    try
    {
        var result = await cache.RetrieveFeedAsync(new Uri(rssFeedUrl));
        Console.WriteLine(result.Name);
        Console.WriteLine($"Total: {result.Items!.Count()}");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
}