using System.Net;
using System.Reflection;
using BliFunc.Library;
using BliFunc.Library.Interfaces;
using BliFunc.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
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

        //// ただのテスト
        //[Function("AiTest")]
        //public async Task<HttpResponseData> TestingValueAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Get)] HttpRequestData req)
        //{
        //    _logger.LogInformation("AIのテスト");

        //    var response = await DonpenAsync(req);

        //    return response;
        //}

        // Donpen.promptyを読み込んで実行する
        [Function("AiDonpen")]
        public async Task<HttpResponseData> DonpenAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Post)] HttpRequestData req)
        {
            _logger.LogInformation("Donpenの実行");

            // リクエストからstringを取得する
            var input = await GetMessageAsync(req);

            // AIを実行する
            var promptyText = function.GerResourceText("BliFunc.Library.AiResources.Prompties.Donpen.prompty");
            var args = new KernelArguments { ["Input"] = input };
            var resultString = await semantic.SimplePromptyAsync(promptyText, args);

            // 結果を返す
            var response = function.AddHeader(req);
            response.WriteString(resultString);
            return response;
        }


        // HttpRequestDataからstringのメッセージを取り出す
        private async Task<string> GetMessageAsync(HttpRequestData req)
        {
            var requestBody = (await new StreamReader(req.Body).ReadToEndAsync());
            return requestBody.Substring(1, requestBody.Length - 2);
        }

    }
}
