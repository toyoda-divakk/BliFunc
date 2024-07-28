```
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

// 構成ファイルを読み込み
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

// カーネルを作成
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        configuration["OpenAI:DeploymentName"]!,
        configuration["OpenAI:Endpoint"]!,
        new AzureCliCredential())
    .Build();

// Prompty ファイルを読み込んで KernelFunction を作成
#pragma warning disable SKEXP0040
var prompty = kernel.CreateFunctionFromPromptyFile("basic.prompty");
#pragma warning restore SKEXP0040

// 実行して結果を表示
var answer = await prompty.InvokeAsync<string>(kernel,
    new KernelArguments
    {
        ["question"] = "クラスの定義方法について教えてください。"
    });
Console.WriteLine(answer);
```



```
---
name: ExamplePrompt
description: This is an example prompty file for the C# QA.
authors:
  - Genkokudo
model:
  api: chat
  parameters:
    max_tokens: 3000
sample:
  question: WinUI3とは何ですか？
---
system:
あなたは C# の質問に答える AI アシスタントです。

user:
{{question}}
```