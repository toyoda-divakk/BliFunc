using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BliFunc.Library.AiResources.Plugins
{
    // promptyからプラグインを作れるかは不明
    // 次はpromptyからプラグインを作ってみよう？


    // 使い方
    // ```
    // kernel.Plugins.AddFromFunctions(
    // "TestPlugin",
    // [PluginExample.CreateNamedFunctionExample(kernel)]); // 関数は複数個を纏めて登録も可能
    // ```
    //
    // このようにしてプラグインとして登録したら、プロンプトから呼び出し
    // 

    // var response = await kernel.InvokePromptAsync("""
    //与えられたテーマに関連する俳句を1つ作成してください。
    //俳句の作成にはテーマに加えて参考情報にある季語も1つ組み込んで作成してください。

    //### テーマ
    //{{ $season }}の朝

    //### 季語
    //{{ TestPlugin.Generate $season }}
    //""",
    //arguments: new KernelArguments
    //{
    //    ["season"] = "春",
    //});


    [Description("季語に関するプラグイン")]
    public class PluginExample
    {
        /// <summary>
        /// プラグインに登録可能な関数の書き方例
        /// </summary>
        /// <param name="kernel"></param>
        /// <returns></returns>
        [KernelFunction]
        [Description("季節の季語を3つ挙げます。")]
        [return: Description("季節に対する3つの季語")]
        public static KernelFunction CreateNamedFunctionExample(Kernel kernel)
        {
            return kernel.CreateFunctionFromPrompt(
                new PromptTemplateConfig("""
                与えられた季節の季語を3つ挙げてください。

                ### 季節
                {{ $season }}
                """)
                {
                    Name = "Generate",  // プロンプトから呼び出すときの名前"{{ TestPlugin.Generate $season }}"
                    InputVariables = [
                        new InputVariable { Name = "season", IsRequired = true, Description="季節" },  // プロンプトから呼び出すときの引数
                    ],
                });
        }
    }


    // クラスごとプラグイン追加するには、以下のようにする
    // インスタンス化してから登録する場合はAddFromObjectを使う。
    //kernel.Plugins.AddFromType<PluginExample>();


    // ※ここから下は使わない予定
    // プロンプトの中から何を使うか、AIに自動で考えて呼んでもらうには
    // CreateFunctionFromPromptの引数で executionSettings: new OpenAIPromptExecutionSettings{ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions} を指定する。
    // #pragma warning disable SKEXP0060が必要。

    // または、プランナー機能を使う。
    //var planner = new HandlebarsPlanner();
    //Console.WriteLine(plan);
    //var plan = await planner.CreatePlanAsync(kernel, """プロンプト""");
    //var result = await plan.InvokeAsync(kernel);
    //Console.WriteLine(result);

    //HandlebarsPlannerは一気に計画を組み立てるので順序を誤ることがある。FunctionCallingStepwisePlannerは1ステップごとにAIに考えてもらうような動きになる。
    //var planner = new FunctionCallingStepwisePlanner();
    //var result = await planner.ExecuteAsync(kernel, """プロンプト""");
    //Console.WriteLine(result.FinalAnswer);

}
