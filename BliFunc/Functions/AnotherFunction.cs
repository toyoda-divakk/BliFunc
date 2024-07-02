using System.Net;
using BliFunc.Library.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BliFunc.Functions
{
    public class AnotherFunction(ILoggerFactory loggerFactory, ISemanticService semantic)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<AnotherFunction>();

        [Function("DiTest")]
        public HttpResponseData DiTest([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString(semantic.Test());

            return response;
        }
    }
}
