using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BliFunc.Library.Interfaces;

/// <summary>
/// 関数の共通処理
/// </summary>
public interface IFunctionService
{
    /// <summary>
    /// 関数共通のヘッダーを追加します。
    /// </summary>
    /// <param name="req"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public HttpResponseData AddHeader(HttpRequestData req, string? message = null);

    /// <summary>
    /// 埋め込みリソースのテキストを取得する
    /// </summary>
    /// <param name="resourceName">"BliFunc.Library.AiResources.Prompties.CommunityToolkit.prompty"</param>
    /// <returns>読み込んだテキスト</returns>
    public string GerResourceText(string resourceName);
}
