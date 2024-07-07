# Azureで使うまで
先にソースをGithubに上げる。（後で関連付けたらなぜか失敗した）
リソースグループに関数アプリを作成。プランは従量課金でよい。
CI/CD設定で、Githubリポジトリを指定。
あとは勝手にやってくれる。

そうしたら関数アプリの「概要」から、関数一覧が出ているので、そこでURLを確認できる。
URLはホストキーと関数キーがある。
ホストキーは全関数共通のキーで、関数キーは個別のキー。
→関数キーを使えばよい。

# DB接続
Azure.CosmosをNugetする。
設定ファイルに、"EndpointUri"と"PrimaryKey"を設定する。この値はAzureDBのクイックスタートから取れた。

# トラブル
## 文字化けする
一番最初からあるFunction.csは、実はSJISで保存されている。

## CosmosDBに登録できない
idとpartitionKeyはCosmosDBの仕様に合わせなければならない。  
JsonProperty属性を使って、camelCaseに変換すること。