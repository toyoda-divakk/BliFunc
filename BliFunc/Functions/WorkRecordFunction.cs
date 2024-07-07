using System.Net;
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

        /// <summary>
        /// リクエストからパーティションキーを取得する
        /// </summary>
        /// <param name="req"></param>
        /// <returns>partitionKeyのクエリパラメータの値</returns>
        private string GetPartitionKey(HttpRequestData req) => string.IsNullOrEmpty(req.Query["partitionKey"]) ? string.Empty : req.Query["partitionKey"]!;

        /// <summary>
        /// 工数を登録する
        /// </summary>
        /// <param name="req">WorkRecord形式のJsonをBodyに持っていること</param>
        /// <returns>結果</returns>
        [Function("RecordWork")]
        public async Task<HttpResponseData> AddAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("工数登録");
            await workRecord.CreateDatabaseAndContainerAsync(); // 存在確認

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var work = JsonConvert.DeserializeObject<WorkRecord>(requestBody);
            if (work == null)
            {
                return function.AddHeader(req, "送信されたデータのデシリアライズができません。");
            }

            var message = await workRecord.AddRecordAsync(work);    // 登録
            return function.AddHeader(req, string.IsNullOrWhiteSpace(message) ? "工数登録が完了しました。" : $"工数登録に失敗しました。:{message}");
        }

        /// <summary>
        /// 指定されたパーティションキーに対する工数の一覧を取得する
        /// クエリパラメータとしてpartitionKeyが必要
        /// </summary>
        /// <param name="req"></param>
        /// <returns>工数の一覧を<List<WorkRecord>のJsonで返す</returns>
        [Function("GetWorkRecords")]
        public async Task<HttpResponseData> GetAsync([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("工数取得");
            await workRecord.CreateDatabaseAndContainerAsync(); // 存在確認

            // 値確認
            string partitionKey = GetPartitionKey(req);
            if (string.IsNullOrEmpty(partitionKey))
            {
                return function.AddHeader(req, "パーティションキーが指定されていません。");
            }

            // データ取得
            var records = await workRecord.GetRecordsAsync(partitionKey);
            if (records == null)
            {
                return function.AddHeader(req, "工数の取得に失敗しました。");
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
        public async Task<HttpResponseData> DeleteAsync([HttpTrigger(AuthorizationLevel.Function, "delete")] HttpRequestData req)
        {
            _logger.LogInformation("工数削除");
            await workRecord.CreateDatabaseAndContainerAsync(); // 存在確認

            // 値確認
            string partitionKey = GetPartitionKey(req);
            if (string.IsNullOrEmpty(partitionKey))
            {
                return function.AddHeader(req, "パーティションキーが指定されていません。");
            }

            // データ取得と削除
            var records = await workRecord.GetRecordsAsync(partitionKey);
            if (records == null)
            {
                return function.AddHeader(req, "工数の取得に失敗しました。");
            }
            foreach (var record in records)
            {
                await workRecord.DeleteAllRecordsAsync(record.PartitionKey);
            }

            return function.AddHeader(req, "工数の削除が完了しました。");
        }

        /// <summary>
        /// 指定されたIDに対するItemを削除する
        /// クエリパラメータとしてidとpartitionKeyが必要
        /// </summary>
        /// <param name="req"></param>
        /// <returns>結果</returns>
        [Function("DeleteWorkRecord")]
        public async Task<HttpResponseData> DeleteByIdAsync([HttpTrigger(AuthorizationLevel.Function, "delete")] HttpRequestData req)
        {
            _logger.LogInformation("工数削除");
            await workRecord.CreateDatabaseAndContainerAsync(); // 存在確認

            // 値確認
            string partitionKey = GetPartitionKey(req);
            if (string.IsNullOrEmpty(partitionKey))
            {
                return function.AddHeader(req, "パーティションキーが指定されていません。");
            }
            string id = req.Query["id"] ?? string.Empty;
            if (string.IsNullOrEmpty(id))
            {
                return function.AddHeader(req, "IDが指定されていません。");
            }

            // データ削除
            var message = await workRecord.DeleteRecordAsync(id, partitionKey);
            return function.AddHeader(req, string.IsNullOrWhiteSpace(message) ? "工数の削除が完了しました。" : $"工数の削除に失敗しました。:{message}");
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
