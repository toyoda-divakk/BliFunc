using System.Net;
using BliFunc.Library;
using BliFunc.Library.Interfaces;
using BliFunc.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BliFunc.Functions
{
    // やりたいことをCosmosDBに放り込んでいこう
    // 作り方はWorkRecordと同じ
    public class TodoFunction(ILoggerFactory loggerFactory, IFunctionService function, ITodoService todo)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<TodoFunction>();
        private readonly string _word = "タスク";

        /// <summary>
        /// リクエストからパーティションキーを取得する
        /// </summary>
        /// <param name="req"></param>
        /// <returns>partitionKeyのクエリパラメータの値</returns>
        private static string GetPartitionKey(HttpRequestData req) => string.IsNullOrEmpty(req.Query[Constants.PartitionKey]) ? string.Empty : req.Query[Constants.PartitionKey]!;

        /// <summary>
        /// データを登録する
        /// </summary>
        /// <param name="req">TodoTask形式のJsonをBodyに持っていること</param>
        /// <returns>結果</returns>
        [Function("RecordTask")]
        public async Task<HttpResponseData> AddAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Post)] HttpRequestData req)
        {
            _logger.LogInformation(string.Format(Constants.LogAdd, _word));
            await todo.CreateDatabaseAndContainerAsync(); // 存在確認

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var record = JsonConvert.DeserializeObject<TodoTask>(requestBody);
            if (record == null)
            {
                return function.AddHeader(req, Constants.DeserializeFailed);
            }

            var message = await todo.AddAsync(record);    // 登録
            return function.AddHeader(req, string.IsNullOrWhiteSpace(message) ? string.Format(Constants.AddSucceed, _word) : string.Format(Constants.AddSucceed, _word, message));
        }

        /// <summary>
        /// 指定されたパーティションキーに対するデータの一覧を取得する
        /// クエリパラメータとしてpartitionKeyが必要
        /// </summary>
        /// <param name="req"></param>
        /// <returns>データの一覧を<List<todo>のJsonで返す</returns>
        [Function("GetTasks")]
        public async Task<HttpResponseData> GetAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Get)] HttpRequestData req)
        {
            _logger.LogInformation(string.Format(Constants.LogGet, _word));
            await todo.CreateDatabaseAndContainerAsync(); // 存在確認

            // 値確認
            var partitionKey = GetPartitionKey(req);
            if (string.IsNullOrEmpty(partitionKey))
            {
                return function.AddHeader(req, Constants.PartitionKeyFailed);
            }

            // データ取得
            var records = await todo.GetAsync(partitionKey);
            if (records == null)
            {
                return function.AddHeader(req, string.Format(Constants.GetFailed, _word));
            }

            var response = function.AddHeader(req);
            response.WriteString(JsonConvert.SerializeObject(records));
            return response;
        }

        /// <summary>
        /// 指定されたパーティションキーに対するItemを全て削除する
        /// クエリパラメータとしてpartitionKeyが必要
        /// </summary>
        /// <param name="req"></param>
        /// <returns>結果</returns>
        [Function("DeleteTasks")]
        public async Task<HttpResponseData> DeleteAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Delete)] HttpRequestData req)
        {
            _logger.LogInformation(string.Format(Constants.LogDeleteAll, _word));
            await todo.CreateDatabaseAndContainerAsync(); // 存在確認

            // 値確認
            string partitionKey = GetPartitionKey(req);
            if (string.IsNullOrEmpty(partitionKey))
            {
                return function.AddHeader(req, Constants.PartitionKeyFailed);
            }

            // データ取得と削除
            var records = await todo.GetAsync(partitionKey);
            if (records == null)
            {
                return function.AddHeader(req, string.Format(Constants.GetFailed, _word));
            }
            foreach (var record in records)
            {
                await todo.DeleteAllAsync(record.PartitionKey);
            }

            return function.AddHeader(req, string.Format(Constants.DeleteSucceed, _word));
        }

        /// <summary>
        /// 指定されたIDに対するItemを削除する
        /// クエリパラメータとしてidとpartitionKeyが必要
        /// </summary>
        /// <param name="req"></param>
        /// <returns>結果</returns>
        [Function("DeleteTask")]
        public async Task<HttpResponseData> DeleteByIdAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Delete)] HttpRequestData req)
        {
            _logger.LogInformation(string.Format(Constants.LogDelete, _word));
            await todo.CreateDatabaseAndContainerAsync(); // 存在確認

            // 値確認
            string partitionKey = GetPartitionKey(req);
            if (string.IsNullOrEmpty(partitionKey))
            {
                return function.AddHeader(req, Constants.PartitionKeyFailed);
            }
            string id = req.Query[Constants.Id] ?? string.Empty;
            if (string.IsNullOrEmpty(id))
            {
                return function.AddHeader(req, Constants.Id);
            }

            // データ削除
            var message = await todo.DeleteAsync(id, partitionKey);
            return function.AddHeader(req, string.IsNullOrWhiteSpace(message) ? string.Format(Constants.DeleteSucceed, _word) : string.Format(Constants.DeleteFailed, _word, message));
        }

        /// <summary>
        /// 指定されたIndexに対するItemを削除する
        /// クエリパラメータとしてindexとpartitionKeyが必要
        /// </summary>
        /// <param name="req"></param>
        /// <returns>結果</returns>
        [Function("DeleteByIndex")]
        public async Task<HttpResponseData> DeleteByIndexAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Delete)] HttpRequestData req)
        {
            _logger.LogInformation(string.Format(Constants.LogDelete, _word));
            await todo.CreateDatabaseAndContainerAsync(); // 存在確認

            // 値確認
            string partitionKey = GetPartitionKey(req);
            if (string.IsNullOrEmpty(partitionKey))
            {
                return function.AddHeader(req, Constants.PartitionKeyFailed);
            }
            string index = req.Query[Constants.Index] ?? string.Empty;
            if (string.IsNullOrEmpty(index))
            {
                return function.AddHeader(req, Constants.Index);
            }

            // データ削除
            var message = await todo.DeleteByIndexAsync(index, partitionKey);
            return function.AddHeader(req, string.IsNullOrWhiteSpace(message) ? string.Format(Constants.DeleteSucceed, _word) : string.Format(Constants.DeleteFailed, _word, message));
        }

        /// <summary>
        /// 登録されているタスクのカテゴリ一覧を取得する
        /// </summary>
        /// <returns>カテゴリ一覧</returns>
        [Function("GetTaskCategories")]
        public async Task<HttpResponseData> GetCategoriesAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Get)] HttpRequestData req)
        {
            var result = await todo.GetPartitionKeysAsync();
            var response = function.AddHeader(req);
            response.WriteString(JsonConvert.SerializeObject(result));
            return response;
        }
    }
}
