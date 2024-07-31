using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BliFunc.Library.AiResources.Plugins
{

    // ネイティブ関数を持ったプラグインの定義
    // プラグインの登録は Kernel の Plugins.AddFromXXXX
    // すると、プロンプトから"{{ プラグイン名.関数名 引数名='値' 引数名='値'}}"で呼び出せるようになる


    // この場合は、Kernel.Plugins.AddFromObject(new UtilsExample);みたいな感じで登録できる。
    [Description("四則演算プラグイン")]    // これを書くと、エージェント機能で利用されるようになるので書いた方が良い。System.ComponentModelをusingする
    public class UtilsExample(TimeProvider timeProvider)
    {
        // 現在時間を返す
        // {{ UtilsExample.LocalNow }}
        [KernelFunction]
        public string LocalNow() => timeProvider.GetLocalNow().ToString("u");   // "u"は"yyyy-MM-dd HH:mm:ss"の書式にする。

        // 2つの数値を足す
        // {{ UtilsExample.Add x='1' y='2' }}
        [KernelFunction]
        [Description("足し算を行います。")]    // これを書くと、エージェント機能で利用されるようになるので書いた方が良い。
        [return: Description("計算結果")]
        public int Add([Description("左辺値")] int x, [Description("右辺値")] int y) => x + y;
    }


    // 関数化したプロンプトって何だっけ？

    // 関数化したプロンプトもプラグインとして登録できる。その場合はAddFromFunctionsを使う。
    // kernel.Plugins.AddFromFunctions("TestPlugin", [func1]);  // 複数の関数が登録できる。
    // CreateNamedFunctionExampleはName = "Generate"なので、"{{ TestPlugin.Generate $season }}"で呼び出せる

    // エージェント機能は、CreateFunctionFromPromptで、executionSettings: new OpenAIPromptExecutionSettings{ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions} を設定すると自動的にやってくれるようになる。




}
