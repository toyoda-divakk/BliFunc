using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BliFunc.Library.Interfaces;

/// <summary>
/// とりあえずSemanticKernelをラッピングする
/// </summary>
public interface ISemanticService
{
    /// <summary>
    /// 接続テスト
    /// </summary>
    /// <param name="settings">API設定</param>
    /// <param name="hello">送信文</param>
    /// <returns>内容</returns>
    Task<string> TestGenerativeAIAsync(IApiSetting settings, string hello);

    ///// <summary>
    ///// Kernelを引数の設定で初期化する。
    ///// </summary>
    ///// <param name="settings"></param>
    //public void Initialize(IApiSetting settings);

    /// <summary>
    /// 設定とプロンプトを指定してチャットを生成する
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="prompt"></param>
    /// <returns></returns>
    ChatHistory InitializeChat(IApiSetting settings, string prompt);

    /// <summary>
    /// チャットを生成する
    /// 履歴に追加する
    /// ※失敗した場合は空文字列を返すので呼び出し元で処理すること
    /// </summary>
    /// <param name="history">今までの会話</param>
    /// <param name="userMessage">ユーザの発言</param>
    /// <returns>返答、失敗した場合は空文字列</returns>
    Task<string> GenerateChatAsync(ChatHistory history, string userMessage);

    /// <summary>
    /// 最後のやり取りを削除して、ユーザが入力したものを返す
    /// ユーザが入力したものを削除しなかったらnull
    /// </summary>
    /// <param name="history"></param>
    /// <returns></returns>
    object? RemoveLastMessage(ChatHistory history);


    Task<string> ExamplePromptTestAsync(IApiSetting settings, string promptyText);

}
