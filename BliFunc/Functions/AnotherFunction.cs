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
        private HttpResponseData AddHeader(HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            return response;
        }

        [Function("DiTest")]
        public HttpResponseData DiTest([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = AddHeader(req);
            response.WriteString(semantic.Test());

            return response;
        }

        // ���ϐ��̃e�X�g
        // local.settings.json�ɐݒ�
        // �T�[�o�[�ł��Y�ꂸ�ɐݒ�
        [Function("TestingValue")]
        public HttpResponseData TestingValue([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("���ϐ��̃e�X�g");

            var response = AddHeader(req);

            var testingValue = Environment.GetEnvironmentVariable("TESTING_VALUE", EnvironmentVariableTarget.Process) ?? "TESTING_VALUE��ݒ肵�Ă��������B";
            response.WriteString(testingValue);

            return response;
        }

        // DB�X�V����ƃL�b�N�����炵���B�R��֐�����Azure��DB�̓�����Azure�֐��̒ǉ��Őݒ�
        [Function("BlizardContainerTrigger")]
        public HttpResponseData BlizardContainerTrigger([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("DB�X�V");

            var response = AddHeader(req);
            response.WriteString("DB�X�V�����炵����");

            return response;
        }



    }
}
