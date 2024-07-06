using System.Net;
using BliFunc.Library.Interfaces;
using BliFunc.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BliFunc.Functions
{
    public class WorkRecordFunction(ILoggerFactory loggerFactory, IFunctionService function, IWorkRecordService workRecord)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<AnotherFunction>();

        [Function("RecordWork")]
        public async Task<HttpResponseData> AddAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req, WorkRecord work)
        {
            _logger.LogInformation("�H���o�^");

            await workRecord.CreateDatabaseAndContainerAsync();

            await workRecord.AddRecordAsync(work);

            return function.AddHeader(req, "�H���o�^���������܂����B");
        }

        [Function("TestDb")]
        public async Task<HttpResponseData> TestDbAsync([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("DB�e�X�g");

            await workRecord.CreateDatabaseAndContainerAsync();

            return function.AddHeader(req, "DB�����܂����B");
        }

        [Function("TestPost")]
        public HttpResponseData TestPost([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req, WorkRecord work)
        {
            _logger.LogInformation("POST�e�X�g");

            return function.AddHeader(req, work.ToString());
        }

        //[Function("AddWorkRecord")]
        //public HttpResponseData DiTest([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        //{
        //    _logger.LogInformation("C# HTTP trigger function processed a request.");

        //    var response = AddHeader(req);
        //    response.WriteString(semantic.Test());

        //    return response;
        //}

        //// ���ϐ��̃e�X�g
        //// local.settings.json�ɐݒ�
        //// �T�[�o�[�ł��Y�ꂸ�ɐݒ�
        //[Function("TestingValue")]
        //public HttpResponseData TestingValue([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        //{
        //    _logger.LogInformation("���ϐ��̃e�X�g");

        //    var response = AddHeader(req);

        //    var testingValue = Environment.GetEnvironmentVariable("TESTING_VALUE", EnvironmentVariableTarget.Process) ?? "TESTING_VALUE��ݒ肵�Ă��������B";
        //    response.WriteString(testingValue);

        //    return response;
        //}

        //// DB�X�V����ƃL�b�N�����炵���B�R��֐�����Azure��DB�̓�����Azure�֐��̒ǉ��Őݒ�
        //[Function("BlizardContainerTrigger")]
        //public HttpResponseData BlizardContainerTrigger([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        //{
        //    _logger.LogInformation("DB�X�V");

        //    var response = AddHeader(req);
        //    response.WriteString("DB�X�V�����炵����");

        //    return response;
        //}
    }
}
