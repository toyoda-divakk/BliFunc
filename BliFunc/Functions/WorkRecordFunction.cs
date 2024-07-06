using System.Net;
using BliFunc.Library.Interfaces;
using BliFunc.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Utf8Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BliFunc.Functions
{
    public class WorkRecordFunction(ILoggerFactory loggerFactory, IFunctionService function, IWorkRecordService workRecord)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<AnotherFunction>();

        [Function("RecordWork")]
        public async Task<HttpResponseData> AddAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("工数登録");

            await workRecord.CreateDatabaseAndContainerAsync();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var work = JsonSerializer.Deserialize<WorkRecord>(requestBody);

            await workRecord.AddRecordAsync(work);

            return function.AddHeader(req, "工数登録が完了しました。");
        }

        [Function("TestDb")]
        public async Task<HttpResponseData> TestDbAsync([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("DBテスト");

            await workRecord.CreateDatabaseAndContainerAsync();

            return function.AddHeader(req, "DBを作りました。");
        }

        //[Function("TestPost")]
        //public HttpResponseData TestPost([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req, WorkRecord work)
        //{
        //    _logger.LogInformation("POSTテスト");

        //    return function.AddHeader(req, work.ToString());
        //}

        [Function("TestPost")]
        public async Task<HttpResponseData> TestPostAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("POSTテスト");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonSerializer.Deserialize<WorkRecord>(requestBody);

            return function.AddHeader(req, data.ToString());
        }

        [Function("Testp")]
        public async Task<HttpResponseData> TestPAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("POSTテスト");
            string requestBody = "";
            try
            {
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonSerializer.Deserialize<WorkRecord>(requestBody);
                return function.AddHeader(req, requestBody);
            }
            catch (Exception ex)
            {
                return function.AddHeader(req, ex.Message);
                throw;
            }
        }

        //[Function("AddWorkRecord")]
        //public HttpResponseData DiTest([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        //{
        //    _logger.LogInformation("C# HTTP trigger function processed a request.");

        //    var response = AddHeader(req);
        //    response.WriteString(semantic.Test());

        //    return response;
        //}

        //// 環境変数のテスト
        //// local.settings.jsonに設定
        //// サーバーでも忘れずに設定
        //[Function("TestingValue")]
        //public HttpResponseData TestingValue([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        //{
        //    _logger.LogInformation("環境変数のテスト");

        //    var response = AddHeader(req);

        //    var testingValue = Environment.GetEnvironmentVariable("TESTING_VALUE", EnvironmentVariableTarget.Process) ?? "TESTING_VALUEを設定してください。";
        //    response.WriteString(testingValue);

        //    return response;
        //}

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
