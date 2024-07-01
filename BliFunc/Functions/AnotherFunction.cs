using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BliFunc.Functions
{
    public class AnotherFunction
    {
        private readonly ILogger _logger;

        public AnotherFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AnotherFunction>();
        }

        [Function("Function5")]
        public HttpResponseData Run2([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Another Hello Work!");

            return response;
        }
    }
}
