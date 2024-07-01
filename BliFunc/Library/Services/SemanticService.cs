using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopAssistant.Core.Contracts.Interfaces;
using DesktopAssistant.Core.Contracts.Services;
using DesktopAssistant.Core.Enums;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace DesktopAssistant.Core.Services;

// https://zenn.dev/microsoft/articles/semantic-kernel-v1-004
// 呼び方
// ・カーネルを作る
// ・InvokePromptAsyncでプロンプトを入れるか、関数を作ってInvokeAsyncで実行する
// ・ChatHistoryを作って、AddUserMessage, AddAssistantMessageで入出力を追加する、IChatCompletionService で生成する。

/// <summary>
/// とりあえずSemanticKernelをラッピングする
/// </summary>
public class SemanticService : ISemanticService
{
    public async Task<string> TestAsync(IApiSetting settings) => await TestGenerativeAIAsync(settings, """Hello, world! と表示する C# のプログラムを書いてください。""");

    private Kernel _kernel = null;

    /// <summary>
    /// APIキーからKernelを作成する
    /// </summary>
    /// <param name="isAzure">Azureならtrue, OpenAIならfalse</param>
    /// <returns></returns>
    private static Kernel Setup(IApiSetting settings)
    {
        var builder = Kernel.CreateBuilder();
        if (settings.GenerativeAI == GenerativeAI.AzureOpenAI)
        {
            builder.AddAzureOpenAIChatCompletion(
                settings.AzureOpenAIModel,
                settings.AzureOpenAIEndpoint,
                settings.AzureOpenAIKey);
        }
        else
        {
            // OpenAIの場合
            builder.AddOpenAIChatCompletion(
                settings.OpenAIModel,
                settings.OpenAIKey);
        }
        return builder.Build();
    }

    /// <summary>
    /// 接続テスト
    /// </summary>
    /// <param name="settings">API設定</param>
    /// <param name="hello">送信文</param>
    /// <returns></returns>
    public async Task<string> TestGenerativeAIAsync(IApiSetting settings, string hello)
    {
        try
        {
            var kernel = Setup(settings);

            // プロンプトを作成
            var prompt = hello;
            var result = await kernel.InvokePromptAsync(prompt);
            return result.GetValue<string>();
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    /// <summary>
    /// Kernelを現在の設定で初期化する。
    /// </summary>
    /// <param name="settings"></param>
    private void Initialize(IApiSetting settings)
    {
        _kernel = Setup(settings);
    }

    /// <summary>
    /// チャットを生成する
    /// 履歴に追加する
    /// ※失敗した場合は空文字列を返すので呼び出し元で処理すること
    /// </summary>
    /// <param name="history">今までの会話</param>
    /// <param name="userMessage">ユーザの発言</param>
    /// <returns>返答、失敗した場合は空文字列</returns>
    public async Task<string> GenerateChatAsync(ChatHistory history, string userMessage)
    {
        var chatService = _kernel.GetRequiredService<IChatCompletionService>();
        history.AddUserMessage(userMessage);
        var response = await chatService.GetChatMessageContentAsync(history);
        if (response.Items.FirstOrDefault() is TextContent responseText)
        {
            history.AddAssistantMessage(responseText.Text);
            return responseText.Text;
        }
        // 失敗した場合最後を削除する
        RemoveLastMessage(history);
        return "";
    }
#nullable enable
    public object? RemoveLastMessage(ChatHistory history)
    {
        // historyが1件以下なら何もせずに終了
        if (history.Count <= 1)
        {
            return null;
        }

        // historyの最後がUserMessageなら、それを削除する
        var last = history.Last();
        
        if (last.Role == AuthorRole.Assistant) {
            // 最後から2件を削除する、最後から2件目の内容を控える
            var last2 = history[^2];
            history.RemoveRange(history.Count - 2, 2);
            return last2.InnerContent;
        }
        else if (last.Role == AuthorRole.User) {
            history.Remove(last);
        }
        // AuthorRole.Toolの時は想定しない
        return null;
    }
#nullable disable

    /// <summary>
    /// 設定とプロンプトを指定してチャットを生成する
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="prompt"></param>
    /// <returns></returns>
    public ChatHistory InitializeChat(IApiSetting settings, string prompt)
    {
        Initialize(settings);
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(prompt);
        return chatHistory;
    }

    #region チャットを作る例
    private static async Task<string> ChatTestAsync(IApiSetting settings)
    {

        // Chat の履歴を作成
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("""
        ソースコードジェネレーターとして振舞ってください。
        ユーザーが指示する内容の C# のソースコードを生成してください。
        生成結果には C# のコード以外を含まないようにしてください。
        """);

        // 入出力例を追加しておく
        chatHistory.AddUserMessage("Hello, world! と表示するプログラムを書いてください。");
        chatHistory.AddAssistantMessage("""
        using System;

        class Program
        {
            static void Main()
            {
                Console.WriteLine("Hello, world!");
            }
        }
        """);
        chatHistory.AddUserMessage("10 * 300 の結果を表示するプログラムを書いてください。");
        chatHistory.AddAssistantMessage("""
        using System;

        class Program
        {
            static void Main()
            {
                Console.WriteLine($"10 * 300 = {10 * 300}");
            }
        }
        """);

        // 本番
        chatHistory.AddUserMessage("与えられた数字が素数かどうか判定するプログラムを書いてください。");

        // IChatCompletionService を使って Chat Completion API を呼び出す
        var kernel = Setup(settings);
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var response = await chatService.GetChatMessageContentAsync(chatHistory);
        if (response.Items.FirstOrDefault() is TextContent responseText)
        {
            return responseText.Text;
        }
        return "No response";
    }
    #endregion

    #region 関数を作る例
    // 例
    //        // テキストを要約する（これは熱力学の例）
    //        var text1 = """
    //熱力学の第1法則 - エネルギーは創造も破壊もできない。
    //熱力学第2法則 - 自然発生的な過程では、宇宙のエントロピーは増大する。
    //熱力学第3法則 - ゼロケルビンの完全な結晶はエントロピーがゼロである。
    //""";

    //var result = await GetSummaryExample(kernel, summarize, text1);
    // Output:
    //   Energy conserved, entropy increases, zero entropy at 0K.

    /// <summary>
    /// 要約を取得する例
    /// </summary>
    /// <param name="kernel"></param>
    /// <param name="summarize">KernelFunction</param>
    /// <param name="text"></param>
    /// <returns></returns>
    private static async Task<FunctionResult> GetSummaryExample(Kernel kernel, KernelFunction summarize, string text) => await kernel.InvokeAsync(summarize, new() { ["input"] = text });

    /// <summary>
    /// プロンプトから関数を作成する例
    /// </summary>
    /// <param name="kernel"></param>
    /// <returns>テキストを要約する関数</returns>
    private static KernelFunction CreateFunctionExample(Kernel kernel)
    {
        // inputじゃなくても、nameとかなんでもOK。argumentsと一致させること。
        var prompt = """
        {{$input}}
        
        One line TLDR with the fewest words.
        """;

        return kernel.CreateFunctionFromPrompt(prompt, executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 100 });
    }

    /// <summary>
    /// プラグインに登録可能な関数の書き方例
    /// </summary>
    /// <param name="kernel"></param>
    /// <returns></returns>
    private static KernelFunction CreateNamedFunctionExample(Kernel kernel)
    {
        return kernel.CreateFunctionFromPrompt(
            new PromptTemplateConfig("""
                与えられた季節の季語を 3 つ挙げてください。

                ### 季節
                {{ $season }}
                """)
            {
                Name = "Generate",  // プロンプトから呼び出すときの名前"{{ TestPlugin.Generate $season }}"
                InputVariables = [
                    new InputVariable { Name = "season", IsRequired = true },
                ],
            });
    }

    #endregion


}

// ネイティブ関数を持ったプラグインの定義
// プラグインの登録は Kernel の Plugins.AddFromXXXX
// すると、プロンプトから"{{ プラグイン名.関数名 引数名='値' 引数名='値'}}"で呼び出せるようになる
// {{ UtilsExample.Add x='1' y='2' }}
// {{ UtilsExample.LocalNow }}

//[Description("四則演算プラグイン")]    // これを書くと、エージェント機能で利用されるようになるので書いた方が良い。System.ComponentModelをusingする
public class UtilsExample(TimeProvider timeProvider) // これはプライマリーコンストラクタというC#12の機能
{
    // 現在時間を返す
    [KernelFunction]
    public string LocalNow() => timeProvider.GetLocalNow().ToString("u");

    // 2つの数値を足す
    [KernelFunction]
    //[Description("足し算を行います。")]    // これを書くと、エージェント機能で利用されるようになるので書いた方が良い。
    //[return: Description("計算結果")]
    public int Add(int x, int y) => x + y;
}

// 関数化したプロンプトもプラグインとして登録できる。その場合はAddFromFunctionsを使う。
// kernel.Plugins.AddFromFunctions("TestPlugin", [func1]);  // 複数の関数が登録できる。
// CreateNamedFunctionExampleはName = "Generate"なので、"{{ TestPlugin.Generate $season }}"で呼び出せる

// エージェント機能は、CreateFunctionFromPromptで、executionSettings: new OpenAIPromptExecutionSettings{ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions} を設定すると自動的にやってくれるようになる。

