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
    // プラグインの登録は Kernel.Plugins.AddFromObject(new UtilsExample);みたいな感じで登録できる。
    // すると、プロンプトから"{{ プラグイン名.関数名 引数名='値' 引数名='値'}}"で呼び出せるようになる
    // 以下の場合だと、{{ UtilsExample.Add x='1' y='2' }}

    [Description("四則演算を行います。")]
    public class UtilsExample(TimeProvider timeProvider)
    {
        [KernelFunction]
        [Description("現在時間を取得します。")]
        public string LocalNow() => timeProvider.GetLocalNow().ToString("u");   // "u"は"yyyy-MM-dd HH:mm:ss"の書式にする。

        [KernelFunction]
        [Description("2つの数値を足します。")]
        [return: Description("計算結果")]
        public int Add([Description("左辺値")] int x, [Description("右辺値")] int y) => x + y;

        [KernelFunction]
        [Description("2つの数値を引きます。")]
        [return: Description("計算結果")]
        public int Subtract([Description("左辺値")] int x, [Description("右辺値")] int y) => x - y;

        [KernelFunction]
        [Description("2つの数値を掛けます。")]
        [return: Description("計算結果")]
        public int Multiply([Description("左辺値")] int x, [Description("右辺値")] int y) => throw new NotImplementedException();

        [KernelFunction]
        [Description("2つの数値を割ります。")]
        [return: Description("計算結果")]
        public double Divide([Description("左辺値")] int x, [Description("右辺値")] int y)
        {
            if (y == 0)
            {
                throw new ArgumentException("右辺値は0であってはいけません。");
            }
            return (double)x / y;
        }
    }



}
