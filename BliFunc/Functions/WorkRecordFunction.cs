using BliFunc.Library;
using BliFunc.Library.Interfaces;
using BliFunc.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BliFunc.Functions
{
    public class WorkRecordFunction(ILoggerFactory loggerFactory, IFunctionService function, IWorkRecordService workRecord)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<WorkRecordFunction>();
        private readonly string _word = "工数";

        /// <summary>
        /// リクエストからパーティションキーを取得する
        /// </summary>
        /// <param name="req"></param>
        /// <returns>partitionKeyのクエリパラメータの値</returns>
        private static string GetPartitionKey(HttpRequestData req) => string.IsNullOrEmpty(req.Query[Constants.PartitionKey]) ? string.Empty : req.Query[Constants.PartitionKey]!;

        /// <summary>
        /// データを登録する
        /// </summary>
        /// <param name="req">WorkRecord形式のJsonをBodyに持っていること</param>
        /// <returns>結果</returns>
        [Function("RecordWork")]
        public async Task<HttpResponseData> AddAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Post)] HttpRequestData req)
        {
            _logger.LogInformation(string.Format(Constants.LogAdd, _word));
            await workRecord.CreateDatabaseAndContainerAsync(); // 存在確認

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var record = JsonConvert.DeserializeObject<WorkRecord>(requestBody);
            if (record == null)
            {
                return function.AddHeader(req, Constants.DeserializeFailed);
            }

            var message = await workRecord.AddAsync(record);    // 登録
            return function.AddHeader(req, string.IsNullOrWhiteSpace(message) ? string.Format(Constants.AddSucceed,  _word) : string.Format(Constants.AddSucceed, _word, message));
        }

        /// <summary>
        /// 指定されたパーティションキーに対するデータの一覧を取得する
        /// クエリパラメータとしてpartitionKeyが必要
        /// </summary>
        /// <param name="req"></param>
        /// <returns>データの一覧を<List<WorkRecord>のJsonで返す</returns>
        [Function("GetWorkRecords")]
        public async Task<HttpResponseData> GetAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Get)] HttpRequestData req)
        {
            _logger.LogInformation(string.Format(Constants.LogGet, _word));
            await workRecord.CreateDatabaseAndContainerAsync(); // 存在確認

            // 値確認
            var partitionKey = GetPartitionKey(req);
            if (string.IsNullOrEmpty(partitionKey))
            {
                return function.AddHeader(req, Constants.PartitionKeyFailed);
            }

            // データ取得
            var records = await workRecord.GetAsync(partitionKey);
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
        [Function("DeleteWorkRecords")]
        public async Task<HttpResponseData> DeleteAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Delete)] HttpRequestData req)
        {
            _logger.LogInformation(string.Format(Constants.LogDeleteAll, _word));
            await workRecord.CreateDatabaseAndContainerAsync(); // 存在確認

            // 値確認
            string partitionKey = GetPartitionKey(req);
            if (string.IsNullOrEmpty(partitionKey))
            {
                return function.AddHeader(req, Constants.PartitionKeyFailed);
            }

            // データ取得と削除
            var records = await workRecord.GetAsync(partitionKey);
            if (records == null)
            {
                return function.AddHeader(req, string.Format(Constants.GetFailed, _word));
            }
            foreach (var record in records)
            {
                await workRecord.DeleteAllAsync(record.PartitionKey);
            }

            return function.AddHeader(req, string.Format(Constants.DeleteSucceed, _word));
        }

        /// <summary>
        /// 指定されたIDに対するItemを削除する
        /// クエリパラメータとしてidとpartitionKeyが必要
        /// </summary>
        /// <param name="req"></param>
        /// <returns>結果</returns>
        [Function("DeleteWorkRecord")]
        public async Task<HttpResponseData> DeleteByIdAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Delete)] HttpRequestData req)
        {
            _logger.LogInformation(string.Format(Constants.LogDelete, _word));
            await workRecord.CreateDatabaseAndContainerAsync(); // 存在確認

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
            var message = await workRecord.DeleteAsync(id, partitionKey);
            return function.AddHeader(req, string.IsNullOrWhiteSpace(message) ? string.Format(Constants.DeleteSucceed, _word) : string.Format(Constants.DeleteFailed, _word, message));
        }


        //// DB更新するとキックされるらしい。蹴る関数名はAzureのDBの統合のAzure関数の追加で設定
        //[Function("BlizardContainerTrigger")]
        //public HttpResponseData BlizardContainerTrigger([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        //{
        //    _logger.LogInformation("DB更新");

        //    var response = AddHeader(req);
        //    response.WriteString("DB更新したらしいよ");

        //    return response;
        //}
    }
}
