using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Locator.Models;

class Program
{
    private static CosmosClient cosmosClient;
    private static Database database;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Azure Maps Locator - Data Injector");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .Build();

        var databaseName = configuration["Locator:DatabaseName"];
        var cosmosEndpoint = configuration.GetConnectionString("CosmosDB");

        var serializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
            IgnoreNullValues = true
        };

        cosmosClient = new CosmosClientBuilder(cosmosEndpoint)
            .WithSerializerOptions(serializerOptions)
            .Build();

        database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);

        await InsertDataAsync<Store>("./Input/Stores.json", "stores", "/country");
        await InsertDataAsync<TagCategory>("./Input/Tags.json", "tags", "/id");

        Console.WriteLine("Done");
    }

    private static async Task InsertDataAsync<T>(string jsonFilePath, string containerId, string partitionKey)
    {
        try
        {
            Console.Write($"{typeof(T).Name}...");

            Container container = await database.CreateContainerIfNotExistsAsync(containerId, partitionKey);

            if (!File.Exists(jsonFilePath))
            {
                Console.WriteLine($"JSON file '{jsonFilePath}' not found. Make sure the file exists and try again.");
                return;
            }

            string jsonContent = File.ReadAllText(jsonFilePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var items = JsonSerializer.Deserialize<List<T>>(jsonContent, options);

            foreach (var item in items)
            {
                await container.UpsertItemAsync(item);
                Console.Write(".");
            }

            Console.WriteLine();
        }
        catch (JsonException)
        {
            Console.WriteLine($"Invalid JSON format in the file '{jsonFilePath}'. Please make sure the JSON is valid.");
        }
        catch (CosmosException ex)
        {
            Console.WriteLine($"Cosmos DB error: {ex.StatusCode} - {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while inserting {typeof(T).Name} data: {ex.Message}");
        }
    }
}
