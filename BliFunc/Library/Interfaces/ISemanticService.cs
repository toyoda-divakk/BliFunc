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
    /// promptyファイルの内容で送信し、応答を1つ得る
    /// 内容の指定などはできない
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="promptyText">promptyファイルの内容</param>
    /// <returns></returns>
    Task<string> SimplePromptyAsync(IApiSetting settings, string promptyText);

    /// <summary>
    /// プロンプト1つだけ送信してその応答を得る
    /// </summary>
    /// <param name="settings">API設定</param>
    /// <param name="prompt">送信プロンプト</param>
    /// <returns></returns>
    Task<string> SimpleGenerateAsync(IApiSetting settings, string hello);

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
    Task<string> GenerateChatAsync(IApiSetting settings, ChatHistory history, string userMessage);

    /// <summary>
    /// 最後のやり取りを削除して、ユーザが入力したものを返す
    /// ユーザが入力したものを削除しなかったらnull
    /// </summary>
    /// <param name="history"></param>
    /// <returns></returns>
    object? RemoveLastMessage(ChatHistory history);

}
