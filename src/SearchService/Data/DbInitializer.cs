using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;

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

        if (count == 0)
        {
            Console.WriteLine("Seeding SearchService database...");
            var itemData = await File.ReadAllTextAsync("Data/auctions.json");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };

            var items = JsonSerializer.Deserialize<List<Item>>(itemData, options)!;

            await DB.InsertAsync(items);
        }
        else
        {
            Console.WriteLine($"SearchService database already seeded...");
        }
    }
}
