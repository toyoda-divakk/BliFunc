using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    // プラグインではない。（プラグイン登録も可能だが、Descriptionを付けること）


    /// <summary>
    /// 関数を作る例と、それを単発で呼び出すメソッドの例
    /// </summary>
    public class FunctionExample
    {
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
        /// 要約を取得する例
        /// </summary>
        /// <param name="kernel"></param>
        /// <param name="summarize">KernelFunction</param>
        /// <param name="text"></param>
        /// <returns></returns>
        private static async Task<FunctionResult> GetSummaryExample(Kernel kernel, KernelFunction summarize, string text) => await kernel.InvokeAsync(summarize, new() { ["input"] = text });

        /// <summary>
        /// プロンプトから関数を作成する例
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
    }
}
