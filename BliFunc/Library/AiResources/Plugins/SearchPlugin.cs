﻿using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BliFunc.Library.AiResources.Plugins;

// Tools を試そう
// Semantic Kernel ではプラグインを自動的にツールに変換してくれる

[Description("インターネット検索に関する機能")]
internal class SearchPlugin
{
    [KernelFunction]
    [Description("インターネットを検索する。")]
    [return: Description("検索結果")]
    public async Task<string> SearchAsync([Description("検索キーワード")] string query)
    {
        Console.WriteLine($"インターネットで検索中: {query}");
        // インターネットにいってる風のスリープ処理
        await Task.Delay(1000);
        return query switch
        {
            _ when query.Contains("ドラゴンクエスト") => "日本の伝統的 RPG。メラが有名",
            _ when query.Contains("ファイナルファンタジー") => "日本の伝統的 RPG。ギルガメッシュが有名。",
            _ => $"{query} に関する情報は見つかりませんでした。",
        };
    }
}


