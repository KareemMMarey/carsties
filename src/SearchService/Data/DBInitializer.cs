using System;
using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data;

public class DBInitializer
{
    public static async Task InitDB(WebApplication app)
    {
        await DB.InitAsync("SearchDB", MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDBConnection")));
        await DB.Index<Item>().Key(x => x.Make, KeyType.Text)
        .Key(x => x.Model, KeyType.Text)
        .Key(x => x.Color, KeyType.Text)
        .CreateAsync();
        var count = await DB.CountAsync<Item>();  
        using var scope = app.Services.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
        var items = await httpClient.GetItemsForSearchDb();
        Console.WriteLine($"{items.Count} items");
        if (items.Count > 0)await DB.SaveAsync(items);
        // if (count == 0){
        //     Console.WriteLine("No Data - will attempt to seed");
        //     var ItemData = await File.ReadAllTextAsync("Data/auction.json");
        //     var options = new JsonSerializerOptions{PropertyNameCaseInsensitive=true};
        //     var items = JsonSerializer.Deserialize<List<Item>>(ItemData, options);
        //     await DB.SaveAsync(items);  
        // }
    }
}
