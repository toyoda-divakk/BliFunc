using BliFunc.Library.Interfaces;
using BliFunc.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BliFunc.Library.Services
{
    public class TodoService(ILoggerFactory loggerFactory) : ITodoService
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<TodoService>();
        private static readonly string? EndpointUri = Environment.GetEnvironmentVariable("CosmosDBEndpointUri");
        private static readonly string? PrimaryKey = Environment.GetEnvironmentVariable("CosmosDBPrimaryKey");
        private static readonly string? DatabaseId = Environment.GetEnvironmentVariable("CosmosDBTodoDatabaseId");
        private static readonly string? ContainerId = Environment.GetEnvironmentVariable("CosmosDBTodoContainerId");

        public async Task<string> AddAsync(TodoTask task)
        {
            _logger.LogInformation("タスク登録");

            using var client = new CosmosClient(EndpointUri, PrimaryKey);
            var database = client.GetDatabase(DatabaseId);
            var container = database.GetContainer(ContainerId);
            try
            {
                ItemResponse<TodoTask> response = await container.CreateItemAsync(task, new PartitionKey(task.PartitionKey));
                _logger.LogInformation($"タスク登録完了。 Resource ID: {response.Resource.Id}");
                return string.Empty;
            }
            catch (CosmosException ex)
            {
                var error = $"タスク登録エラー。Status code: {ex.StatusCode}, Message: {ex.Message}";
                _logger.LogError(error);
                return error;
            }
        }

        public async Task<List<TodoTask>?> GetAsync(string partitionKey)
        {
            using var client = new CosmosClient(EndpointUri, PrimaryKey);
            var database = client.GetDatabase(DatabaseId);
            var container = database.GetContainer(ContainerId);
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.partitionKey = @partitionKey")
                    .WithParameter("@partitionKey", partitionKey);

                var records = new List<TodoTask>();
                using var resultSetIterator = container.GetItemQueryIterator<TodoTask>(query);
                while (resultSetIterator.HasMoreResults)
                {
                    var response = await resultSetIterator.ReadNextAsync();
                    records.AddRange(response.ToList());
                }
                return records;
            }
            catch (CosmosException ex)
            {
                var error = $"タスク取得エラー。Status code: {ex.StatusCode}, Message: {ex.Message}";
                _logger.LogError(error);
                return null;
            }
        }

        public async Task<string> DeleteAllAsync(string partitionKey)
        {
            var records = await GetAsync(partitionKey);
            if (records == null)
            {
                return "タスク取得エラーが発生しました。";
            }

            using var client = new CosmosClient(EndpointUri, PrimaryKey);
            var database = client.GetDatabase(DatabaseId);
            var container = database.GetContainer(ContainerId);
            try
            {
                foreach (var record in records)
                {
                    await container.DeleteItemAsync<TodoTask>(record.Id, new PartitionKey(partitionKey));
                }

                return string.Empty;
            }
            catch (CosmosException ex)
            {
                var error = $"全てのItem削除エラー。Status code: {ex.StatusCode}, Message: {ex.Message}";
                _logger.LogError(error);
                return error;
            }
        }

        public async Task<string> DeleteAsync(string id, string partitionKey)
        {
            using var client = new CosmosClient(EndpointUri, PrimaryKey);
            var database = client.GetDatabase(DatabaseId);
            var container = database.GetContainer(ContainerId);
            try
            {
                await container.DeleteItemAsync<TodoTask>(id, new PartitionKey(partitionKey));
                return string.Empty;
            }
            catch (CosmosException ex)
            {
                var error = $"Item削除エラー。Status code: {ex.StatusCode}, Message: {ex.Message}";
                _logger.LogError(error);
                return error;
            }
        }

        public async Task<string> DeleteByIndexAsync(int index, string partitionKey)
        {
            var records = await GetAsync(partitionKey);
            if (records == null)
            {
                return "タスク取得エラーが発生しました。";
            }
            if (records.Count <= index)
            {
                return "指定されたインデックスのタスクが存在しません。";
            }
            return await DeleteAsync(records[index].Id, partitionKey);
        }

        public async Task CreateDatabaseAndContainerAsync()
        {
            if (string.IsNullOrEmpty(EndpointUri) || string.IsNullOrEmpty(PrimaryKey) || string.IsNullOrEmpty(DatabaseId) || string.IsNullOrEmpty(ContainerId))
            {
                _logger.LogError("環境変数が設定されていません。");
                return;
            }

            using var client = new CosmosClient(EndpointUri, PrimaryKey);
            var database = await client.CreateDatabaseIfNotExistsAsync(DatabaseId);
            var container = await database.Database.CreateContainerIfNotExistsAsync(ContainerId, "/partitionKey");
        }

        public async Task<List<string>> GetPartitionKeysAsync()
        {
            _logger.LogInformation("GetPartitionKeys");
            using var client = new CosmosClient(EndpointUri, PrimaryKey);
            var database = client.GetDatabase(DatabaseId);
            var container = database.GetContainer(ContainerId);

            List<string> partitionKeys = [];
            try
            {
                QueryDefinition queryDefinition = new("SELECT DISTINCT c.partitionKey FROM c");
                FeedIterator<dynamic> queryResultSetIterator = container.GetItemQueryIterator<dynamic>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (var item in currentResultSet)
                    {
                        partitionKeys.Add(item.partitionKey.ToString());
                    }
                }

                return partitionKeys;
            }
            catch (Exception ex)
            {
                var error = $"Item取得エラー。Message: {ex.Message}";
                _logger.LogError(error);
                partitionKeys.Add(ex.Message);
                return partitionKeys;
            }

        }
    }
}
