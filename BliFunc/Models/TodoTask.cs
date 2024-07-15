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
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Todoタスク
        /// </summary>
        /// <param name="category">分類（半角英字で書くこと）</param>
        /// <param name="description">内容</param>
        public TodoTask(string category, string description)
        {
            Description = description;

            Id = Guid.NewGuid().ToString();
            PartitionKey = category;
        }

    }
}
