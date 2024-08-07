using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BliFunc.Library.Interfaces;
using Microsoft.Azure.Functions.Worker.Http;

namespace BliFunc.Library.Services;

/// <summary>
/// 
/// </summary>
public class FunctionService : IFunctionService
{
    public HttpResponseData AddHeader(HttpRequestData req, string? message = null)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        if (message != null)
        {
            response.WriteString(message);
        }
        return response;
    }

    /// <summary>
    /// 埋め込みリソースのテキストを取得する
    /// </summary>
    /// <param name="resourceName">"BliFunc.Library.AiResources.Prompties.CommunityToolkit.prompty"</param>
    /// <returns>読み込んだテキスト</returns>
    public string GerResourceText(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
        using StreamReader reader = new(stream);
        var result = reader.ReadToEnd();
        return result;
    }
}

