using Newtonsoft.Json;

namespace BliFunc.Models
{

    /// <summary>
    /// Todoタスク
    /// </summary>
    public record TodoTask
    {
        /// <summary>
        /// Cosmos DBのドキュメントID
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// パーティションキー
        /// 分類を設定する
        /// </summary>
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; } = string.Empty;

        /// <summary>
        /// タスクの内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        public TodoTask(string category, string content)
        {
            Content = content;

            Id = Guid.NewGuid().ToString();
            PartitionKey = category;
        }

    }
}
