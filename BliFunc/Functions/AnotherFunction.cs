using System.Net;
using BliFunc.Library.Interfaces;
using BliFunc.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BliFunc.Functions
{
    public class AnotherFunction(ILoggerFactory loggerFactory, IFunctionService function)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<AnotherFunction>();


        // 環境変数のテスト
        // local.settings.jsonに設定
        // サーバーでも忘れずに設定
        [Function("TestingValue")]
        public HttpResponseData TestingValue([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("環境変数のテスト");

            var response = function.AddHeader(req);

            var testingValue = Environment.GetEnvironmentVariable("TESTING_VALUE", EnvironmentVariableTarget.Process) ?? "TESTING_VALUEを設定してください。";
            response.WriteString(testingValue);

            return response;
        }

    }
}
