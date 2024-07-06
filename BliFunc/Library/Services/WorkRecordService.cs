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
        private static readonly string? EndpointUri = Environment.GetEnvironmentVariable("CosmosDBEndpointUri");    // 毎回読み込めばいい？
        private static readonly string? PrimaryKey = Environment.GetEnvironmentVariable("CosmosDBPrimaryKey");
        private static readonly string? DatabaseId = Environment.GetEnvironmentVariable("CosmosDBWorkDatabaseId");
        private static readonly string? ContainerId = Environment.GetEnvironmentVariable("CosmosDBWorkContainerId");

        /// <summary>
        /// 工数を登録する
        /// </summary>
        /// <param name="workRecord">工数データ</param>
        /// <returns></returns>
        public async Task<string> AddRecordAsync(WorkRecord workRecord)
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

        /// <summary>
        /// パーティションキーを条件に工数を取得する
        /// </summary>
        /// <param name="partitionKey">パーティションキー</param>
        /// <returns></returns>
        public async Task<List<WorkRecord>?> GetRecordsAsync(string partitionKey)
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

        /// <summary>
        /// Database, Containerの存在を確認し、なければ作成する
        /// </summary>
        /// <returns></returns>
        public async Task CreateDatabaseAndContainerAsync()
        {
            using var client = new CosmosClient(EndpointUri, PrimaryKey);
            var database = await client.CreateDatabaseIfNotExistsAsync(DatabaseId);
            var container = await database.Database.CreateContainerIfNotExistsAsync(ContainerId, "/partitionKey");
        }

    }
}
