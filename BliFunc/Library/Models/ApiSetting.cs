namespace BliFunc.Library.Interfaces;

/// <summary>
/// APIに関する設定項目のインターフェース
/// </summary>
public record ApiSetting : IApiSetting
{
    public string AzureOpenAIKey
    {
        get; set;
    }
    public string AzureOpenAIModel
    {
        get; set;
    }
    public string AzureOpenAIEndpoint
    {
        get; set;
    }

    public ApiSetting()
    {
        AzureOpenAIKey = Environment.GetEnvironmentVariable("AzureOpenAIKey") ?? string.Empty;
        AzureOpenAIModel = Environment.GetEnvironmentVariable("AzureOpenAIModel") ?? string.Empty;
        AzureOpenAIEndpoint = Environment.GetEnvironmentVariable("AzureOpenAIEndpoint") ?? string.Empty;
    }
}
