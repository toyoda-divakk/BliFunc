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
        public async Task<HttpResponseData> TestingValueAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("AIのテスト");

            var response = function.AddHeader(req);
            var promptyText = GerResourceText("BliFunc.Library.AiResources.Prompties.ExamplePrompt.prompty");
            response.WriteString(await semantic.SimplePromptyAsync(new ApiSetting(), promptyText));


            return response;
        }

        /// <summary>
        /// 埋め込みリソースのテキストを取得する
        /// </summary>
        /// <param name="resourceName">"BliFunc.Library.AiResources.Prompties.CommunityToolkit.prompty"</param>
        /// <returns>読み込んだテキスト</returns>
        private static string GerResourceText(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
            using StreamReader reader = new(stream);
            var result = reader.ReadToEnd();
            return result;
        }
    }
}
