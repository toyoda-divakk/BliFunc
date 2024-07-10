namespace BliFunc.Library
{
    public class Constants
    {
        public const string Get = "get";
        public const string Post = "post";
        public const string Delete = "delete";

        public static readonly string PartitionKey = "partitionKey";
        public static readonly string Id = "id";

        public static readonly string DeserializeFailed = "送信されたデータのデシリアライズができません。";
        public static readonly string PartitionKeyFailed = "パーティションキーが指定されていません。";
        public static readonly string IdFailed = "IDが指定されていません。";

        public static readonly string AddSucceed = "{0}の登録が完了しました。";
        public static readonly string AddFailed = "{0}の登録に失敗しました。:{1}";
        public static readonly string DeleteSucceed = "{0}の削除が完了しました。";
        public static readonly string DeleteFailed = "{0}の削除に失敗しました。:{1}";
        public static readonly string GetFailed = "{0}の取得に失敗しました。";

        public static readonly string LogGet = "{0}取得";
        public static readonly string LogAdd = "{0}登録";
        public static readonly string LogDeleteAll = "{0}全削除";
        public static readonly string LogDelete = "{0}削除";
    }
}
