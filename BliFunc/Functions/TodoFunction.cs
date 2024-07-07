using System.Net;
using BliFunc.Library.Interfaces;
using BliFunc.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BliFunc.Functions
{
    // Ç‚ÇËÇΩÇ¢Ç±Ç∆ÇCosmosDBÇ…ï˙ÇËçûÇÒÇ≈Ç¢Ç±Ç§
    // çÏÇËï˚ÇÕWorkRecordÇ∆ìØÇ∂
    public class TodoFunction(ILoggerFactory loggerFactory, IFunctionService function, ITodoService todo)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<TodoFunction>();

    }
}
