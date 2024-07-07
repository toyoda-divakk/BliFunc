using System.Net;
using BliFunc.Library.Interfaces;
using BliFunc.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BliFunc.Functions
{
    // ��肽�����Ƃ�CosmosDB�ɕ��荞��ł�����
    // ������WorkRecord�Ɠ���
    public class TodoFunction(ILoggerFactory loggerFactory, IFunctionService function, ITodoService todo)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<TodoFunction>();

    }
}
