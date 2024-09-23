using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BliFunc.Library.Interfaces;

/// <summary>
/// とりあえずSemanticKernelをラッピングする
/// </summary>
public interface ISemanticService
{
    /// <summary>
    /// promptyファイルの内容と単一の入力を送信し、応答を1つ得る
    /// 引数はInput固定
    /// </summary>
    /// <param name="promptyText">promptyファイルの内容</param>
    /// <param name="input">AIへの入力</param>
    /// <param name="settings">API設定</param>
    /// <returns></returns>
    Task<string> SimplePromptyAsync(string promptyText, string input, IApiSetting? settings = null);

    /// <summary>
    /// promptyファイルの内容と単一の入力を送信し、応答を1つ得る
    /// </summary>
    /// <param name="promptyText">promptyファイルの内容</param>
    /// <param name="input">AIへの入力</param>
    /// <param name="settings">API設定</param>
    /// <returns></returns>
    Task<string> SimplePromptyAsync(string promptyText, KernelArguments input, IApiSetting? settings = null);



    // Note:チャットはローカルに持たせるから、AzureFunctionでは保持しないよね。
    /// <summary>
    /// 設定とプロンプトを指定してチャットを生成する
    /// </summary>
    /// <param name="prompt">送信プロンプト</param>
    /// <param name="settings">API設定</param>
    /// <returns></returns>
    ChatHistory InitializeChat(string prompt, IApiSetting? settings = null);

    /// <summary>
    /// チャットを生成する
    /// 履歴に追加する
    /// ※失敗した場合は空文字列を返すので呼び出し元で処理すること
    /// </summary>
    /// <param name="history">今までの会話</param>
    /// <param name="userMessage">ユーザの発言</param>
    /// <param name="settings"></param>
    /// <returns>返答、失敗した場合は空文字列</returns>
    Task<string> GenerateChatAsync(ChatHistory history, string userMessage, IApiSetting? settings = null);

    /// <summary>
    /// 最後のやり取りを削除して、ユーザが入力したものを返す
    /// ユーザが入力したものを削除しなかったらnull
    /// </summary>
    /// <param name="history"></param>
    /// <returns></returns>
    object? RemoveLastMessage(ChatHistory history);

}
