using BliFunc.Models;

namespace BliFunc.Library.Interfaces;

/// <summary>
/// DBにタスクを記録する処理
/// </summary>
public interface ITodoService
{
    /// <summary>
    /// Itemを登録する
    /// </summary>
    /// <param name="item"></param>
    /// <returns>正常ならempty、異常ならエラーメッセージ</returns>
    Task<string> AddAsync(TodoTask item);

    /// <summary>
    /// パーティションキーを条件にItemを取得する
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
