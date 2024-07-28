namespace BliFunc.Library.Interfaces;

/// <summary>
/// APIに関する設定項目のインターフェース
/// </summary>
public interface IApiSetting
{
    /// <summary>
    /// AzureOpenAIのAPIキー
    /// </summary>
    string AzureOpenAIKey
    {
        get;
    }
    /// <summary>
    /// AzureOpenAIのデプロイメント名
    /// </summary>
    string AzureOpenAIModel
    {
        get;
    }
    /// <summary>
    /// AzureOpenAIのエンドポイント
    /// </summary>
    string AzureOpenAIEndpoint
    {
        get;
    }
}
