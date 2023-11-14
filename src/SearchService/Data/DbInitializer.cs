using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {
        await DB.InitAsync(
            "SearchDb",
            MongoClientSettings.FromConnectionString(
                app.Configuration.GetConnectionString("MongoDbConnection")
            )
        );

        await DB.Index<Item>().Key(it => it.Make, KeyType.Text).CreateAsync();

        await DB.Index<Item>().Key(it => it.Model, KeyType.Text).CreateAsync();

        await DB.Index<Item>().Key(it => it.Color, KeyType.Text).CreateAsync();

        var result = await DB.Index<Item>()
            .Key(it => it.Make, KeyType.Text)
            .Key(it => it.Model, KeyType.Text)
            .Key(it => it.Color, KeyType.Text)
            .CreateAsync();

        Console.WriteLine(result);

        var count = await DB.CountAsync<Item>();

        using var scope = app.Services.CreateScope();

        var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();

        var items = await httpClient.GetItemsForSearchDb();

        Console.WriteLine($"Received {items?.Count ?? -1} items from AuctionService");

        if (items is null || items.Count == 0)
            return;

        await DB.SaveAsync(items);
    }
}
