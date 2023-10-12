// Azure Maps Store Locator (version 1.0-rc.1)
// Copyright (c) Microsoft Corporation. All rights reserved.
// https://github.com/Azure-Samples/Azure-Maps-Locator
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using StoreLocator.Models;

class Program
{
    private static CosmosClient cosmos;
    private static Database database;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Azure Maps Store Locator - Demo Data Injector");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .Build();

        var databaseName = configuration["Database:Name"];
        var connectionString = configuration["Database:ConnectionString"];
        var serializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
            IgnoreNullValues = true
        };

        cosmos = new CosmosClientBuilder(connectionString)
            .WithSerializerOptions(serializerOptions)
            .Build();

        database = await cosmos.CreateDatabaseIfNotExistsAsync(databaseName);

        await InsertDataAsync<Store>("./Data/stores.json", "stores", "/address/countryCode");
        await InsertDataAsync<Feature>("./Data/features.json", "features", "/id");
        await InsertDataAsync<Country>("./Data/countries.json", "countries", "/id");

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
        catch (JsonException ex)
        {
            Console.WriteLine($"Invalid JSON format in the file '{jsonFilePath}'. Please make sure the JSON is valid. - {ex.Message}");
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
