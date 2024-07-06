using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BliFunc.Library.Enums;
using BliFunc.Library.Interfaces;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

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
}

