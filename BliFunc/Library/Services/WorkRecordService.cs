using BliFunc.Library.Interfaces;
using BliFunc.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BliFunc.Library.Services
{
    public class WorkRecordService(ILoggerFactory loggerFactory) : IWorkRecordService
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<WorkRecordService>();
        private static readonly string? EndpointUri = Environment.GetEnvironmentVariable("CosmosDBEndpointUri");
        private static readonly string? PrimaryKey = Environment.GetEnvironmentVariable("CosmosDBPrimaryKey");
        private static readonly string? DatabaseId = Environment.GetEnvironmentVariable("CosmosDBWorkDatabaseId");
        private static readonly string? ContainerId = Environment.GetEnvironmentVariable("CosmosDBWorkContainerId");

        public async Task<string> AddAsync(WorkRecord workRecord)
        {
            _logger.LogInformation("工数登録");

            using var client = new CosmosClient(EndpointUri, PrimaryKey);
            var database = client.GetDatabase(DatabaseId);
            var container = database.GetContainer(ContainerId);
            try
            {
                ItemResponse<WorkRecord> response = await container.CreateItemAsync(workRecord, new PartitionKey(workRecord.PartitionKey));
                _logger.LogInformation($"工数登録完了。 Resource ID: {response.Resource.Id}");
                return string.Empty;
            }
            catch (CosmosException ex)
            {
                var error = $"工数登録エラー。Status code: {ex.StatusCode}, Message: {ex.Message}";
                _logger.LogError(error);
                return error;
            }
        }

        public async Task<List<WorkRecord>?> GetAsync(string partitionKey)
        {
            using var client = new CosmosClient(EndpointUri, PrimaryKey);
            var database = client.GetDatabase(DatabaseId);
            var container = database.GetContainer(ContainerId);
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.partitionKey = @partitionKey")
                    .WithParameter("@partitionKey", partitionKey);

                var records = new List<WorkRecord>();
                using var resultSetIterator = container.GetItemQueryIterator<WorkRecord>(query);
                while (resultSetIterator.HasMoreResults)
                {
                    var response = await resultSetIterator.ReadNextAsync();
                    records.AddRange(response.ToList());
                }
                return records;
            }
            catch (CosmosException ex)
            {
                var error = $"工数取得エラー。Status code: {ex.StatusCode}, Message: {ex.Message}";
                _logger.LogError(error);
                return null;
            }
        }

        public async Task<string> DeleteAllAsync(string partitionKey)
        {
            var records = await GetAsync(partitionKey);
            if (records == null)
            {
                return "工数取得エラーが発生しました。";
            }

            using var client = new CosmosClient(EndpointUri, PrimaryKey);
            var database = client.GetDatabase(DatabaseId);
            var container = database.GetContainer(ContainerId);
            try
            {
                foreach (var record in records)
                {
                    await container.DeleteItemAsync<WorkRecord>(record.Id, new PartitionKey(partitionKey));
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
                await container.DeleteItemAsync<WorkRecord>(id, new PartitionKey(partitionKey));
                return string.Empty;
            }
            catch (CosmosException ex)
            {
                var error = $"Item削除エラー。Status code: {ex.StatusCode}, Message: {ex.Message}";
                _logger.LogError(error);
                return error;
            }
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

    }
}
