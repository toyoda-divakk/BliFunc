using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BliFunc.Models;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BliFunc.Library.Interfaces;

/// <summary>
/// DBにタスクを記録する処理
/// </summary>
public interface ITodoService
{
    /// <summary>
    /// タスクを登録する
    /// </summary>
    /// <param name="task"></param>
    /// <returns>正常ならempty、異常ならエラーメッセージ</returns>
    Task<string> AddAsync(TodoTask task);

    /// <summary>
    /// パーティションキーを条件にタスクを取得する
    /// </summary>
    /// <param name="partitionKey">パーティションキー</param>
    /// <returns>エラーならnull</returns>
    Task<List<TodoTask>?> GetAsync(string partitionKey);

    /// <summary>
    /// Database, Containerの存在を確認し、なければ作成する
    /// </summary>
    /// <returns></returns>
    Task CreateDatabaseAndContainerAsync();

    /// <summary>
    /// パーティションキーを条件に全てのItemを削除する
    /// </summary>
    /// <param name="partitionKey">パーティションキー</param>
    /// <returns>正常ならempty、異常ならエラーメッセージ</returns>
    Task<string> DeleteAllAsync(string partitionKey);

    /// <summary>
    /// IDを条件にItemを削除する
    /// </summary>
    /// <param name="id">ID</param>
    /// <param name="partitionKey">パーティションキー</param>
    /// <returns>正常ならempty、異常ならエラーメッセージ</returns>
    Task<string> DeleteAsync(string id, string partitionKey);
}
