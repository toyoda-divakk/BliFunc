using System.Net;
using System.Reflection;
using BliFunc.Library;
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

        // ただのテスト
        [Function("AiTest")]
        public async Task<HttpResponseData> TestingValueAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Get)] HttpRequestData req)
        {
            _logger.LogInformation("AIのテスト");

            var response = function.AddHeader(req);
            var promptyText = function.GerResourceText("BliFunc.Library.AiResources.Prompties.ExamplePrompt.prompty");
            response.WriteString(await semantic.SimplePromptyAsync(promptyText));

            return response;
        }

        // Donpen.promptyを読み込んで実行する
        [Function("AiDonpen")]
        public async Task<HttpResponseData> DonpenAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Post)] HttpRequestData req)
        {
            _logger.LogInformation("Donpenの実行");
            // リクエストからstringを取得
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();


            var response = function.AddHeader(req);
            //var promptyText = function.GerResourceText("BliFunc.Library.AiResources.Prompties.Donpen.prompty");
            //response.WriteString(await semantic.SimplePromptyAsync(promptyText));

            response.WriteString(requestBody);
            return response;
        }   // ※これだと実行できない。入力が無いから。ExamplePromptを参照してuserの入力を定義すること。




    }
}
