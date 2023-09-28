using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using StoreLocator.Helpers;
using StoreLocator.Models;
using System.Net;

namespace StoreLocator.Services
{
    public class DataServices
    {
        private Container _storesContainer;
        private Container _tagsContainer;
        private readonly CosmosClient _cosmosClient;

        public DataServices(IConfiguration configuration)
        {
            var databaseName = configuration["Database:Name"];
            var connectionString = configuration["Database:ConnectionString"];
            var serializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                IgnoreNullValues = true
            };

            _cosmosClient = new CosmosClientBuilder(connectionString)
                .WithSerializerOptions(serializerOptions)
                .Build();

            InitializeDatabaseAndContainer(databaseName).Wait();
        }

        private async Task InitializeDatabaseAndContainer(string databaseName)
        {
            // Check if the database exists
            DatabaseResponse database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);

            // Create a container if it doesn't exist
            _storesContainer = await database.Database.CreateContainerIfNotExistsAsync("stores", "/country");
            _tagsContainer = await database.Database.CreateContainerIfNotExistsAsync("tags", "/id");
        }

        // TODO: Add methods to create, update, and delete stores

        public async Task<List<Store>> GetAllStoresAsync()
        {
            var queryText = "SELECT * FROM s";
            var queryDefinition = new QueryDefinition(queryText);

            return await QueryStoresAsync<Store>(queryDefinition);
        }

        public async Task<List<TagCategory>> GetAllTagsAsync()
        {
            var queryText = "SELECT * FROM t";
            var queryDefinition = new QueryDefinition(queryText);

            return await QueryTagsAsync<TagCategory>(queryDefinition);
        }

        public async Task<Store> GetStoreByIdAsync(string id)
        {
            var queryText = "SELECT * FROM s WHERE s.id = @id";
            var queryDefinition = new QueryDefinition(queryText)
                .WithParameter("@id", id);

            return (await QueryStoresAsync<Store>(queryDefinition)).FirstOrDefault();
        }

        public async Task<List<StoreWithDistance>> GetStoresBySearchAsync(string query, int? limit, string country, string tags, double? latitude, double? longitude, double? rangeInKm)
        {
            var sqlQuery = "SELECT * FROM s WHERE ";
            var queryParams = new List<(string, object)>();
            var parameterCount = 0;

            if (!string.IsNullOrEmpty(query))
            {
                parameterCount++;

                var searchText = query.ToLower();
                sqlQuery += "LOWER(s.name) LIKE @name OR LOWER(s.city) LIKE @city";
                queryParams.Add(("@name", $"%{searchText}%"));
                queryParams.Add(("@city", $"{searchText}%"));
            }

            if (!string.IsNullOrEmpty(country))
            {
                parameterCount++;

                if (queryParams.Any())
                    sqlQuery += " AND ";

                sqlQuery += "LOWER(s.country) = @country";
                queryParams.Add(("@country", country.ToLower()));
            }

            if (!string.IsNullOrEmpty(tags))
            {
                parameterCount++;

                var tagCount = 0;
                var tagsArray = tags.Split(',');

                foreach (var tag in tagsArray)
                {
                    if (queryParams.Any())
                        sqlQuery += " AND ";

                    tagCount++;

                    sqlQuery += $"ARRAY_CONTAINS(s.tags, @tag{tagCount})";
                    queryParams.Add(($"@tag{tagCount}", tag.ToLower()));
                }
            }

            if (longitude.HasValue && latitude.HasValue && rangeInKm.HasValue)
            {
                parameterCount++;

                if (queryParams.Any())
                    sqlQuery += " AND ";

                sqlQuery += "ST_DISTANCE(s.location, { 'type': 'Point', 'coordinates': [@longitude, @latitude] }) <= @rangeInKm * 1000";

                queryParams.Add(("@latitude", latitude));
                queryParams.Add(("@longitude", longitude));
                queryParams.Add(("@rangeInKm", rangeInKm));
            }

            // if no parameters are specified, return all stores
            if (parameterCount == 0)
            {
                sqlQuery += " 1 = 1";
            }

            if (limit.HasValue)
            {
                sqlQuery += " OFFSET 0 LIMIT @limit";
                queryParams.Add(("@limit", limit.Value));
            }

            var queryDefinition = new QueryDefinition(sqlQuery);

            foreach (var (paramName, paramValue) in queryParams)
            {
                queryDefinition.WithParameter(paramName, paramValue);
            }

            var stores = await QueryStoresAsync<StoreWithDistance>(queryDefinition);

            if (longitude.HasValue && latitude.HasValue)
            {
                foreach (var store in stores)
                {
                    store.DistanceInKm = GeospatialHelper.CalculateDistanceInKm((double)latitude, (double)longitude, store.Location.Coordinates[1], store.Location.Coordinates[0]);
                }
            }

            return stores;
        }

        public async Task DeleteStoreAsync(Store store)
        {
            var response = await _storesContainer.DeleteItemAsync<Store>(store.Id, new PartitionKey(store.Country));

            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new Exception($"Failed to delete store. Status code: {response.StatusCode}");
            }
        }

        public async Task UpsertStore(Store store)
        {
            var response = await _storesContainer.UpsertItemAsync<Store>(store);

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"Failed to save store. Status code: {response.StatusCode}");
            }
        }

        private async Task<List<T>> QueryStoresAsync<T>(QueryDefinition queryDefinition)
        {
            var queryResult = _storesContainer.GetItemQueryIterator<T>(queryDefinition);
            var items = new List<T>();

            while (queryResult.HasMoreResults)
            {
                var response = await queryResult.ReadNextAsync();
                items.AddRange(response);
            }

            return items;
        }

        private async Task<List<T>> QueryTagsAsync<T>(QueryDefinition queryDefinition)
        {
            var queryResult = _tagsContainer.GetItemQueryIterator<T>(queryDefinition);
            var items = new List<T>();

            while (queryResult.HasMoreResults)
            {
                var response = await queryResult.ReadNextAsync();
                items.AddRange(response);
            }

            return items;
        }
    }
}