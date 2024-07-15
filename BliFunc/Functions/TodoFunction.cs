using System.Net;
using BliFunc.Library;
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
        private readonly string _word = "�^�X�N";

        /// <summary>
        /// ���N�G�X�g����p�[�e�B�V�����L�[���擾����
        /// </summary>
        /// <param name="req"></param>
        /// <returns>partitionKey�̃N�G���p�����[�^�̒l</returns>
        private static string GetPartitionKey(HttpRequestData req) => string.IsNullOrEmpty(req.Query[Constants.PartitionKey]) ? string.Empty : req.Query[Constants.PartitionKey]!;

        /// <summary>
        /// �f�[�^��o�^����
        /// </summary>
        /// <param name="req">TodoTask�`����Json��Body�Ɏ����Ă��邱��</param>
        /// <returns>����</returns>
        [Function("RecordTask")]
        public async Task<HttpResponseData> AddAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Post)] HttpRequestData req)
        {
            _logger.LogInformation(string.Format(Constants.LogAdd, _word));
            await todo.CreateDatabaseAndContainerAsync(); // ���݊m�F

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var record = JsonConvert.DeserializeObject<TodoTask>(requestBody);
            if (record == null)
            {
                return function.AddHeader(req, Constants.DeserializeFailed);
            }

            var message = await todo.AddAsync(record);    // �o�^
            return function.AddHeader(req, string.IsNullOrWhiteSpace(message) ? string.Format(Constants.AddSucceed, _word) : string.Format(Constants.AddSucceed, _word, message));
        }

        /// <summary>
        /// �w�肳�ꂽ�p�[�e�B�V�����L�[�ɑ΂���f�[�^�̈ꗗ���擾����
        /// �N�G���p�����[�^�Ƃ���partitionKey���K�v
        /// </summary>
        /// <param name="req"></param>
        /// <returns>�f�[�^�̈ꗗ��<List<todo>��Json�ŕԂ�</returns>
        [Function("GetTasks")]
        public async Task<HttpResponseData> GetAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Get)] HttpRequestData req)
        {
            _logger.LogInformation(string.Format(Constants.LogGet, _word));
            await todo.CreateDatabaseAndContainerAsync(); // ���݊m�F

            // �l�m�F
            var partitionKey = GetPartitionKey(req);
            if (string.IsNullOrEmpty(partitionKey))
            {
                return function.AddHeader(req, Constants.PartitionKeyFailed);
            }

            // �f�[�^�擾
            var records = await todo.GetAsync(partitionKey);
            if (records == null)
            {
                return function.AddHeader(req, string.Format(Constants.GetFailed, _word));
            }

            var response = function.AddHeader(req);
            response.WriteString(JsonConvert.SerializeObject(records));
            return response;
        }

        /// <summary>
        /// �w�肳�ꂽ�p�[�e�B�V�����L�[�ɑ΂���Item��S�č폜����
        /// �N�G���p�����[�^�Ƃ���partitionKey���K�v
        /// </summary>
        /// <param name="req"></param>
        /// <returns>����</returns>
        [Function("DeleteTasks")]
        public async Task<HttpResponseData> DeleteAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Delete)] HttpRequestData req)
        {
            _logger.LogInformation(string.Format(Constants.LogDeleteAll, _word));
            await todo.CreateDatabaseAndContainerAsync(); // ���݊m�F

            // �l�m�F
            string partitionKey = GetPartitionKey(req);
            if (string.IsNullOrEmpty(partitionKey))
            {
                return function.AddHeader(req, Constants.PartitionKeyFailed);
            }

            // �f�[�^�擾�ƍ폜
            var records = await todo.GetAsync(partitionKey);
            if (records == null)
            {
                return function.AddHeader(req, string.Format(Constants.GetFailed, _word));
            }
            foreach (var record in records)
            {
                await todo.DeleteAllAsync(record.PartitionKey);
            }

            return function.AddHeader(req, string.Format(Constants.DeleteSucceed, _word));
        }

        /// <summary>
        /// �w�肳�ꂽID�ɑ΂���Item���폜����
        /// �N�G���p�����[�^�Ƃ���id��partitionKey���K�v
        /// </summary>
        /// <param name="req"></param>
        /// <returns>����</returns>
        [Function("DeleteTask")]
        public async Task<HttpResponseData> DeleteByIdAsync([HttpTrigger(AuthorizationLevel.Function, Constants.Delete)] HttpRequestData req)
        {
            _logger.LogInformation(string.Format(Constants.LogDelete, _word));
            await todo.CreateDatabaseAndContainerAsync(); // ���݊m�F

            // �l�m�F
            string partitionKey = GetPartitionKey(req);
            if (string.IsNullOrEmpty(partitionKey))
            {
                return function.AddHeader(req, Constants.PartitionKeyFailed);
            }
            string id = req.Query[Constants.Id] ?? string.Empty;
            if (string.IsNullOrEmpty(id))
            {
                return function.AddHeader(req, Constants.Id);
            }

            // �f�[�^�폜
            var message = await todo.DeleteAsync(id, partitionKey);
            return function.AddHeader(req, string.IsNullOrWhiteSpace(message) ? string.Format(Constants.DeleteSucceed, _word) : string.Format(Constants.DeleteFailed, _word, message));
        }


    }
}
