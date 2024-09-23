using System.Net;
using System.Reflection;
using Azure.AI.OpenAI;
using BliFunc.Library;
using BliFunc.Library.AiResources.Plugins;
using BliFunc.Library.Interfaces;
using BliFunc.Library.Services;
using BliFunc.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;

namespace BliFunc.Functions
{
    // Semantic Kernelを使ったFunction Callingをやってみる

    /// <summary>
    /// 実験用クラス
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="function"></param>
    /// <param name="semantic"></param>
    public class LaboFunction(ILoggerFactory loggerFactory, IFunctionService function, ISemanticService semantic)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<AiFunction>();

        /// <summary>
        /// プラグインをツールとして扱って、自動的に呼び出すテスト
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Function("ToolAutoUseTest")]
        public async Task<HttpResponseData> ToolAutoUseTestAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Get)] HttpRequestData req)
        {
            var kernel = SemanticService.Setup(new ApiSetting());
            kernel.Plugins.AddFromType<SearchPlugin>(); // プラグインを登録

            // プラグインをツールとして扱って、自動的に呼び出して結果を返すようにする
            var settings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            };

            var resultString = await kernel.InvokePromptAsync<string>(
                "ドラゴンクエストでは何が有名ですか？インターネットで検索して教えてください。",       // このように指定すると、SearchPluginに"ドラゴンクエスト 有名な点"というクエリが渡される。すげー
                new KernelArguments(settings)
            );

            var res = function.AddHeader(req);
            res.WriteString(resultString ?? "失敗しました");
            return res;
        }

        // 更新系のプラグインなどは勝手に呼び出されると困るケースが多いので処理を自分で書く
        /// <summary>
        /// プラグインをツールとして扱って、呼び出すツールを返してもらい、それを呼び出すコードを自分で書くテスト
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Function("ToolManualUseTest")]
        public async Task<HttpResponseData> ToolManualUseTestAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Get)] HttpRequestData req)
        {
            var kernel = SemanticService.Setup(new ApiSetting());
            kernel.Plugins.AddFromType<SearchPlugin>(); // プラグインを登録

            // プラグインをツールとして扱う設定
            var settings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions,
            };

            // こっちはInvokePromptAsyncだと空文字列が戻ってくるだけ。ツールに対応する情報を現時点だと指定できないので ChatHistory を使う
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage("ドラゴンクエストでは何が有名ですか？インターネットで検索して教えてください。");

            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            var response = (OpenAIChatMessageContent)await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
            chatHistory.Add(response);  // 履歴にレスポンスを追加

            // メッセージが返ってきている場合は表示
            if (!string.IsNullOrEmpty(response.Content))
            {
                Console.WriteLine(response.Content);
            }

            // 頑張ってツールを呼び出すところ
            foreach (var toolCall in response.ToolCalls.OfType<ChatCompletionsFunctionToolCall>()) // toolCallには、{ "query":"ドラゴンクエスト 有名な点"}    // "SearchPlugin-Search"というデータが入っている
            {
                if (kernel.Plugins.TryGetFunctionAndArguments(toolCall, out var function, out var arguments))   // function:"インターネットを検索する。"というDescriptionの内容が入る
                {
                    var toolResponse = await kernel.InvokeAsync<string>(function, arguments) ?? "";
                    Console.WriteLine($"検索結果: {toolResponse}");

                    // ツールの結果を履歴に保存
                    chatHistory.Add(new ChatMessageContent(
                        AuthorRole.Tool,
                        toolResponse,       // "日本の伝統的 RPG。メラが有名"という関数の実行結果を単純に格納
                        metadata: new Dictionary<string, object?>
                        {
                            [OpenAIChatMessageContent.ToolIdProperty] = toolCall.Id,    // ここはおまじないで。
                        }));
                }
            }
            
            // 会話履歴にツールの実行結果を入れた状態でそのまま投げる。
            // ここでもツール呼び出しを期待する場合は settings や kernel を渡す
            var finalAnswer = await chatService.GetChatMessageContentAsync(chatHistory);
            var resultString = finalAnswer.Content;



            var res = function.AddHeader(req);
            res.WriteString(resultString ?? "失敗しました");
            return res;
        }
    }
}
