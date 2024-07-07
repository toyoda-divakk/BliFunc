using System.Net;
using BliFunc.Library.Interfaces;
using BliFunc.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BliFunc.Functions
{
    // やりたいことをCosmosDBに放り込んでいこう
    // 作り方はWorkRecordと同じ
    public class TodoFunction(ILoggerFactory loggerFactory, IFunctionService function, ITodoService todo)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<TodoFunction>();

    }
}
