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

// Promptyファイルはこのプロジェクトに持たせる。

namespace BliFunc.Functions
{
    public class AiFunction(ILoggerFactory loggerFactory, IFunctionService function, ISemanticService semantic)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<AiFunction>();

        /// <summary>
        /// リクエストからNameを取得する
        /// </summary>
        /// <param name="req"></param>
        /// <returns>Nameのクエリパラメータの値</returns>
        private static string GetName(HttpRequestData req) => string.IsNullOrEmpty(req.Query[Constants.Name]) ? string.Empty : req.Query[Constants.Name]!;

        // Donpen.promptyを読み込んで実行する
        [Function("AiDonpen")]
        public async Task<HttpResponseData> DonpenAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Post)] HttpRequestData req)
        {
            return await SimpleCallAiAsync(req, "Donpen");
        }
        [Function("AiComment")]
        public async Task<HttpResponseData> CommentAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Post)] HttpRequestData req)
        {
            return await SimpleCallAiAsync(req, "EnglishComment");
        }
        [Function("AiCommunity")]
        public async Task<HttpResponseData> CommunityAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Post)] HttpRequestData req)
        {
            return await SimpleCallAiAsync(req, "CommunityToolkit");
        }

        // Aiで使用できるname一覧を取得します。
        [Function("AiList")]
        public HttpResponseData AiList([HttpTrigger(AuthorizationLevel.Function, Constants.Get)] HttpRequestData req)
        {
            var response = function.AddHeader(req);
            response.WriteString("Donpen\nEnglishComment\nCommunityToolkit");
            return response;
        }

        /// <summary>
        /// 任意のプロンプトを読み込んで実行する
        /// クエリパラメータにプロンプト名を指定する
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Function("Ai")]
        public async Task<HttpResponseData> SimpleAiAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Post)] HttpRequestData req)
        {
            var name = GetName(req);
            if (string.IsNullOrEmpty(name))
            {
                return function.AddHeader(req, Constants.NameFailed);
            }

            return await SimpleCallAiAsync(req, name);
        }

        /// <summary>
        /// 単一のAIプロンプトを実行して、AI応答を格納したレスポンスを得る
        /// ユーザ入力はリクエストから取得する
        /// プロンプトはpromptyファイルを使用する
        /// 入力は"Input"固定とする
        /// </summary>
        /// <param name="req"></param>
        /// <param name="promptyName">promptyファイル名、拡張子不要</param>
        /// <returns>AI応答を格納したレスポンス</returns>
        private async Task<HttpResponseData> SimpleCallAiAsync(HttpRequestData req, string promptyName)
        {
            // リクエストからstringを取得する
            var input = await GetMessageAsync(req);

            // AIを実行する
            var resultString = await SimpleCallAiAsync(promptyName, input);

            // 結果を返す
            var response = function.AddHeader(req);
            response.WriteString(resultString);
            return response;
        }


        /// <summary>
        /// 単一のAIプロンプトを実行する
        /// プロンプトはpromptyファイルを使用する
        /// 入力は"Input"固定とする
        /// </summary>
        /// <param name="promptyName">promptyファイル名、拡張子不要</param>
        /// <param name="input">ユーザー入力</param>
        /// <returns>AIの応答</returns>
        private async Task<string> SimpleCallAiAsync(string promptyName, string input)
        {
            _logger.LogInformation($"{promptyName}の実行");

            var promptyText = function.GerResourceText($"BliFunc.Library.AiResources.Prompties.{promptyName}.prompty");
            var args = new KernelArguments { ["Input"] = input };
            return await semantic.SimplePromptyAsync(promptyText, args);
        }


        // HttpRequestDataからstringのメッセージを取り出す
        private async Task<string> GetMessageAsync(HttpRequestData req)
        {
            var requestBody = (await new StreamReader(req.Body).ReadToEndAsync());
            return requestBody.Substring(1, requestBody.Length - 2);
        }

    }
}
