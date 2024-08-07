using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BliFunc.Library.AiResources.Plugins
{
    // KernelってFunction Callingと関係あるの？
    // https://zenn.dev/microsoft/articles/azure-openai-add-function-calling
    // →違うみたい。向こうはOpenAIClientからFunctionDefinitionを登録して使う。
    // Kernelはエージェントに実行順序を考えてもらうというやり方で、ChatGPTのFunction Callingとは違うアプローチ。

    // こっちを参考にする。
    // Function calling を使った Planner とかは無いので Semantic Kernel が提供する IChatCompletion インターフェースを使って利用する形になります。
    // https://zenn.dev/microsoft/articles/semantic-kernel-18


    // これを作っておくと、await GetSummaryExample(kernel, SummarizeExample(kernel), text1);で、text1の内容を要約できる。
    // 単発呼び出し。
    // プラグインと作り方が一緒なので、プラグインとして作っておくと良い。

    // Function Callingを作っておくと、手順が決まり切ったタスクを作りやすい。

    // 公式ドキュメント
    // https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling?pivots=programming-language-csharp

    /// <summary>
    /// 関数を作る例と、それを単発で呼び出すメソッドの例
    /// </summary>
    public class FunctionExample
    {
        // Kernelを使って関数を呼び出す例（Function Callingではない）
        // この例では、要約する関数を作成して、それを呼び出す例を示しています。

        // var text1 = """
        //熱力学の第1法則 - エネルギーは創造も破壊もできない。
        //熱力学第2法則 - 自然発生的な過程では、宇宙のエントロピーは増大する。
        //熱力学第3法則 - ゼロケルビンの完全な結晶はエントロピーがゼロである。
        //""";

        //var result = await GetSummaryExample(kernel, SummarizeExample(kernel), text1);
        // Output:
        //   Energy conserved, entropy increases, zero entropy at 0K.

        /// <summary>
        /// 要約を取得
        /// </summary>
        /// <param name="kernel"></param>
        /// <param name="summarize">KernelFunction</param>
        /// <param name="text"></param>
        /// <returns></returns>
        private static async Task<FunctionResult> GetSummaryExample(Kernel kernel, KernelFunction summarize, string text) => await kernel.InvokeAsync(summarize, new() { ["input"] = text });

        // プロンプトから関数を作成する例
        /// <summary>
        /// テキストを要約する関数を返す
        /// </summary>
        /// <param name="kernel"></param>
        /// <returns>テキストを要約する関数</returns>
        private static KernelFunction SummarizeExample(Kernel kernel)
        {
            // inputじゃなくても、nameとかなんでもOK。argumentsと一致させること。
            var prompt = """
        {{$input}}
        
        One line TLDR with the fewest words.
        """;

            return kernel.CreateFunctionFromPrompt(prompt, executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 100 });
        }

        // この方法でも定義できる
        //[KernelFunction("get_pizza_menu")]
        //[Description("Get pizza menu")]
        //public async Task<Menu> GetPizzaMenuAsync()
        //{
        //    return await pizzaService.GetMenu();
        //}


        // プラグイン登録方法
        // TODO:IKernelBuilderを使った方法にSemanticServiceを変更する
        // 注意：関数が増えるとトークンも増えるので、機能ごとに追加するプラグインは必要最小限にすること。
        // プラグインは依存性注入を使用するので、プラグインのコンストラクタに依存性を追加することができる。 // builder.Services.AddSingleton
        // https://learn.microsoft.com/en-us/semantic-kernel/concepts/kernel?pivots=programming-language-csharp#using-dependency-injection

        //IKernelBuilder kernelBuilder = new KernelBuilder();
        //kernelBuilder..AddAzureOpenAIChatCompletion(
        //    deploymentName: "NAME_OF_YOUR_DEPLOYMENT",
        //    apiKey: "YOUR_API_KEY",
        //    endpoint: "YOUR_AZURE_ENDPOINT"
        //);
        //kernelBuilder.Plugins.AddFromType<OrderPizzaPlugin>("OrderPizza");
        //Kernel kernel = kernelBuilder.Build();

        // あとはKernelを以下のように設定。
        //IChatCompletionService chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

        //OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        //{
        //    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        //};

        //ChatResponse response = await chatCompletion.GetChatMessageContentAsync(
        //    chatHistory,
        //    executionSettings: openAIPromptExecutionSettings,
        //    kernel: kernel)

    }
}
