using Newtonsoft.Json;

namespace BliFunc.Models
{

    /// <summary>
    /// 日付と工数
    /// </summary>
    public record WorkRecord
    {
        /// <summary>
        /// Cosmos DBのドキュメントID
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// パーティションキー
        /// 年月を6桁の文字列で表現したもの。
        /// </summary>
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; } = string.Empty;

        /// <summary>
        /// 作業を行った日付を記録します。
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 期間を示す場合のみ使用。
        /// 作業が終わった日付を記録します。
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 作業に費やした時間（工数）を小数点で記録します。
        /// </summary>
        public double Hours { get; set; }

        /// <summary>
        /// 実行したタスクの名前や説明を記録します。
        /// </summary>
        public string TaskName { get; set; }

        ///// <summary>
        ///// ユーザーを識別するための ID を記録します。
        ///// </summary>
        //public string UserId { get; set; } = string.Empty;    // 1人で使うからいらない

        public WorkRecord(string taskName, double hours, DateTime? date = null, DateTime? endDate = null)
        {
            Date = date == null ? DateTime.UtcNow : date.Value;
            EndDate = endDate;
            Hours = hours;

            TaskName = int.TryParse(taskName, out _) ? $"改修作業[Gitlab Task No.{taskName}]" : taskName;    // 特別仕様

            Id = Guid.NewGuid().ToString();
            PartitionKey = GetPartitionKey(Date);
        }

        // 月次集計の際、検索に使用する
        public static string GetPartitionKey(DateTime date)
        {
            return date.ToString("yyyyMM");
        }

        /// <summary>
        /// 報告書用フォーマットに変換します。
        /// </summary>
        /// <returns></returns>
        public string ToSheetFormat()
        {
            // "7/6\tissue番号:1234\t7.75"のように表示する
            return EndDate == null || Date.Day == EndDate.Value.Day ? $"{Date:M/d}\t{TaskName}\t{Hours}" : $"{Date:M/d}-{EndDate.Value.Day:d}\t{TaskName}\t{Hours}";
        }
    }
}
