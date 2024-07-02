using System.Net;
using BliFunc.Library.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BliFunc.Functions
{
    public class AnotherFunction(ILoggerFactory loggerFactory, ISemanticService semantic)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<AnotherFunction>();
        private HttpResponseData AddHeader(HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            return response;
        }

        [Function("DiTest")]
        public HttpResponseData DiTest([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = AddHeader(req);
            response.WriteString(semantic.Test());

            return response;
        }

        // 環境変数のテスト
        // local.settings.jsonに設定
        // サーバーでも忘れずに設定
        [Function("TestingValue")]
        public HttpResponseData TestingValue([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("環境変数のテスト");

            var response = AddHeader(req);

            var testingValue = Environment.GetEnvironmentVariable("TESTING_VALUE", EnvironmentVariableTarget.Process) ?? "TESTING_VALUEを設定してください。";
            response.WriteString(testingValue);

            return response;
        }

        // DB更新するとキックされるらしい。蹴る関数名はAzureのDBの統合のAzure関数の追加で設定
        [Function("BlizardContainerTrigger")]
        public HttpResponseData BlizardContainerTrigger([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("DB更新");

            var response = AddHeader(req);
            response.WriteString("DB更新したらしいよ");

            return response;
        }



    }
}
