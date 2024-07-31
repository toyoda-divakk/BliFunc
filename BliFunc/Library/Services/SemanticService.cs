using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using BliFunc.Library.Interfaces;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BliFunc.Library.Services;

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
    public async Task<string> SimplePromptyAsync(IApiSetting settings, string promptyText)      // TODO: IApiSettingっていちいち送らなきゃダメ？固定なのに？
    {
        var kernel = Setup(settings);

#pragma warning disable SKEXP0040 // 種類は、評価の目的でのみ提供されています。将来の更新で変更または削除されることがあります。続行するには、この診断を非表示にします。
        var prompty = kernel.CreateFunctionFromPrompty(promptyText);
#pragma warning restore SKEXP0040

        var answer = await prompty.InvokeAsync<string>(kernel,
        new KernelArguments
        {
            ["question"] = "クラスの定義方法について教えてください。"
        }) ?? string.Empty;
        return answer;

        #region Streamingにする場合はこっち
        //await foreach (var chunk in kernel.InvokeStreamingAsync<string>(
        //    prompty,
        //    new() { ["question"] = "クラス定義方法について教えてください。" }))
        //{
        //    Console.Write(chunk);
        //}
        //return "";
        #endregion
    }


    /// <summary>
    /// APIキーからKernelを作成する
    /// </summary>
    /// <param name="isAzure">Azureならtrue, OpenAIならfalse</param>
    /// <returns></returns>
    private static Kernel Setup(IApiSetting settings)
    {
        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(
            settings.AzureOpenAIModel,
            settings.AzureOpenAIEndpoint,
            settings.AzureOpenAIKey);

        #region OpenAIの場合
        //builder.AddOpenAIChatCompletion(
        //    settings.OpenAIModel,
        //    settings.OpenAIKey);
        #endregion

        return builder.Build();
    }

    /// <summary>
    /// プロンプト1つだけ送信してその応答を得る
    /// </summary>
    /// <param name="settings">API設定</param>
    /// <param name="prompt">送信プロンプト</param>
    /// <returns></returns>
    public async Task<string> SimpleGenerateAsync(IApiSetting settings, string prompt)
    {
        try
        {
            var kernel = Setup(settings);
            var result = await kernel.InvokePromptAsync(prompt);
            return result.GetValue<string>()!;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }













    /// <summary>
    /// チャットを生成する
    /// 履歴に追加する
    /// ※失敗した場合は空文字列を返すので呼び出し元で処理すること
    /// </summary>
    /// <param name="history">今までの会話</param>
    /// <param name="userMessage">ユーザの発言</param>
    /// <returns>返答、失敗した場合は空文字列</returns>
    public async Task<string> GenerateChatAsync(IApiSetting settings, ChatHistory history, string userMessage)
    {
        var kernel = Setup(settings);
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        history.AddUserMessage(userMessage);
        var response = await chatService.GetChatMessageContentAsync(history);
        if (response.Items.FirstOrDefault() is TextContent responseText)
        {
            history.AddAssistantMessage(responseText.Text!);
            return responseText.Text!;
        }
        // 失敗した場合最後を削除する
        RemoveLastMessage(history);
        return "";
    }
#nullable enable
    // 最後のやり取りを削除して、ユーザが入力したものを返す
    public object? RemoveLastMessage(ChatHistory history)
    {
        // historyが1件以下なら何もせずに終了
        if (history.Count <= 1)
        {
            return null;
        }

        // historyの最後がUserMessageなら、それを削除する
        var last = history.Last();

        if (last.Role == AuthorRole.Assistant)
        {
            // 最後から2件を削除する、最後から2件目の内容を控える
            var last2 = history[^2];
            history.RemoveRange(history.Count - 2, 2);
            return last2.InnerContent;
        }
        else if (last.Role == AuthorRole.User)
        {
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


}



