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
/// DBに工数を記録する処理
/// </summary>
public interface IWorkRecordService
{
    /// <summary>
    /// 工数を登録する
    /// </summary>
    /// <param name="workRecord">工数データ</param>
    /// <returns>エラーメッセージ（成功時は空文字）</returns>
    Task<string> AddRecordAsync(WorkRecord workRecord);

    /// <summary>
    /// パーティションキーを条件に工数を取得する
    /// </summary>
    /// <param name="partitionKey">パーティションキー</param>
    /// <returns>WorkRecordのリスト（エラー時はnull）</returns>
    Task<List<WorkRecord>?> GetRecordsAsync(string partitionKey);

    /// <summary>
    /// Database, Containerの存在を確認し、なければ作成する
    /// </summary>
    /// <returns></returns>
    Task CreateDatabaseAndContainerAsync();
}
