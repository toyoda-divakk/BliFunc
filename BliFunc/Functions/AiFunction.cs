using System.Net;
using System.Reflection;
using BliFunc.Library.Interfaces;
using BliFunc.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// AzureFunctionってメモリ的なの無いよね？会話履歴どうするの？
// 会話履歴はAzure Cosmos DBに保存することにしよう
// ただし、毎回読み書きはしない。開始時と終了時のみ。
// 途中の会話履歴は、クライアントに保持する。

// Promptyファイルはどう保存する？
// DBじゃなくファイルで。このプロジェクトに持たせる。

namespace BliFunc.Functions
{
    public class AiFunction(ILoggerFactory loggerFactory, IFunctionService function, ISemanticService semantic)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<AiFunction>();

        [Function("AiTest")]
        public HttpResponseData TestingValue([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("AIのテスト");

            var response = function.AddHeader(req);

            var testingValue = Environment.GetEnvironmentVariable("TESTING_VALUE", EnvironmentVariableTarget.Process) ?? "TESTING_VALUEを設定してください。";
            response.WriteString(testingValue);

            return response;
        }

        [Function("FileLoadTest")]
        public HttpResponseData FileLoadTest([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("ファイル読み込みのテスト");

            var response = function.AddHeader(req);

            // リソースのCommunityToolkit.promptyを読み込む
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "BliFunc.Library.AiResources.Prompties.CommunityToolkit.prompty";
            using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
            using StreamReader reader = new(stream);
            string result = reader.ReadToEnd();

            response.WriteString(result);

            return response;
        }
    }
}
