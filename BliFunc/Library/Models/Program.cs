//using System;
//using System.Threading.Tasks;
//using System.Configuration;
//using System.Collections.Generic;
//using System.Net;
//using Microsoft.Azure.Cosmos;

//namespace BliFunc.Library.Models
//{
//    // んー？データベースがあって、その中にパーティションキーで名づけられたコンテナが複数あって、その中にパーティションキーで名づけられたアイテムが複数ある？

//    class Program
//    {
//        // エンドポイント
//        private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"];       // 環境変数からとること

//        // なんかものすごい文字列が入る
//        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];       // 環境変数からとること

//        // クライアント
//        private CosmosClient cosmosClient;

//        // DB本体
//        private Database database;

//        // コンテナ（って何？）
//        private Container container;

//        // 作成するデータベースとコンテナの名前。
//        private string databaseId = "ToDoList";
//        private string containerId = "Items";

//        public static async Task Main(string[] args)
//        {
//            try
//            {
//                var p = new Program();
//                await p.GetStartedDemoAsync();  // cosmosClientに繋いでいろいろ作成、実行

//            }
//            catch (CosmosException de)
//            {
//                Exception baseException = de.GetBaseException();
//                Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine("Error: {0}", e);
//            }
//            finally
//            {
//                Console.WriteLine("End of demo, press any key to exit.");
//                Console.ReadKey();
//            }
//        }

//        /// <summary>
//        /// このサンプルで Azure Cosmos DB リソースを操作するメソッドを呼び出すエントリポイント
//        /// </summary>
//        public async Task GetStartedDemoAsync()
//        {
//            // Cosmosクライアントの新しいインスタンスを作成します。
//            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });   // ApplicationNameとは？
//            await CreateDatabaseAsync();    // データベースが存在しない場合は作成
//            await CreateContainerAsync();   // コンテナが存在しない場合は作成（パーティション・キーを入れてリクエストと保存の分散とかいうことをやってる）
//            await ScaleContainerAsync();    // コンテナのスループットをスケール（なにそれ）
//            await AddItemsToContainerAsync();   // ファミリーのアイテムをコンテナに追加する、無かったら作成、あったら何もしない
//            await QueryItemsAsync();                // クエリの実行。キーはパーティションキーを使っている（じゃあIDは？）
//            await ReplaceFamilyItemAsync(); // コンテナ内のアイテムを置き換える（更新する）
//            await DeleteFamilyItemAsync();  // コンテナ内の項目を削除する
//            await DeleteDatabaseAndCleanupAsync();  // データベースを削除し、Cosmosクライアントインスタンスを破棄
//        }

//        /// <summary>
//        /// データベースが存在しない場合は作成する
//        /// </summary>
//        private async Task CreateDatabaseAsync()
//        {
//            // 新しいデータベースを作成する
//            database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
//            Console.WriteLine("Created Database: {0}\n", database.Id);
//        }

//        /// <summary>
//        /// コンテナが存在しない場合は作成する。
//        /// ファミリー情報を保存するため、パーティション・キー・パスとして"/partitionKey "を指定し、リクエストと保存の分散を図る。（どういうこと？一時領域みたいな？）
//        /// </summary>
//        /// <returns></returns>
//        private async Task CreateContainerAsync()
//        {
//            container = await database.CreateContainerIfNotExistsAsync(containerId, "/partitionKey");
//            Console.WriteLine("Created Container: {0}\n", container.Id);
//        }

//        /// <summary>
//        /// 既存のコンテナにプロビジョニングされたスループットをスケールします。
//        /// ワークロードのニーズに合わせて、コンテナのスループット（RU/s）を上下させることができる。 Learn more: https://aka.ms/cosmos-request-units
//        /// </summary>
//        /// <returns></returns>
//        private async Task ScaleContainerAsync()
//        {
//            // 現在のスループットを読む
//            try
//            {
//                int? throughput = await container.ReadThroughputAsync();
//                if (throughput.HasValue)
//                {
//                    Console.WriteLine("Current provisioned throughput : {0}\n", throughput.Value);
//                    int newThroughput = throughput.Value + 100;
//                    // スループットの更新
//                    await container.ReplaceThroughputAsync(newThroughput);
//                    Console.WriteLine("New provisioned throughput : {0}\n", newThroughput);
//                }
//            }
//            catch (CosmosException cosmosException) when (cosmosException.StatusCode == HttpStatusCode.BadRequest)
//            {
//                Console.WriteLine("Cannot read container throuthput.");
//                Console.WriteLine(cosmosException.ResponseBody);
//            }

//        }

//        /// <summary>
//        /// ファミリーのアイテムをコンテナに追加する
//        /// </summary>
//        private async Task AddItemsToContainerAsync()
//        {
//            // アンデルセン家のファミリーオブジェクトを作成する
//            var andersenFamily = new Family
//            {
//                Id = "Andersen.1",
//                PartitionKey = "Andersen",          // パーティションキー（ってなんだろう）
//                LastName = "Andersen",
//                Parents =
//                [
//                    new Parent { FirstName = "Thomas" },
//                    new Parent { FirstName = "Mary Kay" }
//                ],
//                Children =
//                [
//                    new Child
//                    {
//                        FirstName = "Henriette Thaulow",
//                        Gender = "female",
//                        Grade = 5,
//                        Pets =
//                        [
//                            new Pet { GivenName = "Fluffy" }
//                        ]
//                    }
//                ],
//                Address = new Address { State = "WA", County = "King", City = "Seattle" },
//                IsRegistered = false
//            };

//            try
//            {
//                // その項目が存在するかどうかを読む。
//                ItemResponse<Family> andersenFamilyResponse = await container.ReadItemAsync<Family>(andersenFamily.Id, new PartitionKey(andersenFamily.PartitionKey));      // パーティションキーってなに？※見つからなかったら例外に飛ぶそうな
//                Console.WriteLine("Item in database with id: {0} already exists\n", andersenFamilyResponse.Resource.Id);
//            }
//            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
//            {
//                // Andersenファミリーを表すアイテムをコンテナに作成する。このアイテムのパーティション・キーの値は "Andersen"です。
//                ItemResponse<Family> andersenFamilyResponse = await container.CreateItemAsync(andersenFamily, new PartitionKey(andersenFamily.PartitionKey));

//                // アイテムを作成した後、ItemResponse の Resource プロパティでアイテムのボディにアクセスできることに注意してください。また、RequestChargeプロパティにアクセスして、このリクエストで消費されたRUの量を確認することもできます。
//                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", andersenFamilyResponse.Resource.Id, andersenFamilyResponse.RequestCharge);
//            }

//            // ウェイクフィールド家のファミリーオブジェクトを作成する
//            var wakefieldFamily = new Family
//            {
//                Id = "Wakefield.7",
//                PartitionKey = "Wakefield",
//                LastName = "Wakefield",
//                Parents =
//                [
//                    new Parent { FamilyName = "Wakefield", FirstName = "Robin" },
//                    new Parent { FamilyName = "Miller", FirstName = "Ben" }
//                ],
//                Children =
//                [
//                    new Child
//                    {
//                        FamilyName = "Merriam",
//                        FirstName = "Jesse",
//                        Gender = "female",
//                        Grade = 8,
//                        Pets =
//                        [
//                            new Pet { GivenName = "Goofy" },
//                            new Pet { GivenName = "Shadow" }
//                        ]
//                    },
//                    new Child
//                    {
//                        FamilyName = "Miller",
//                        FirstName = "Lisa",
//                        Gender = "female",
//                        Grade = 1
//                    }
//                ],
//                Address = new Address { State = "NY", County = "Manhattan", City = "NY" },
//                IsRegistered = true
//            };

//            try
//            {
//                // アイテムが存在するかどうかを読み取る
//                ItemResponse<Family> wakefieldFamilyResponse = await container.ReadItemAsync<Family>(wakefieldFamily.Id, new PartitionKey(wakefieldFamily.PartitionKey));
//                Console.WriteLine("Item in database with id: {0} already exists\n", wakefieldFamilyResponse.Resource.Id);
//            }
//            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
//            {
//                // Wakefieldファミリーを表すアイテムをコンテナ内に作成する。このアイテムのパーティション・キーの値は "Wakefield"である。
//                ItemResponse<Family> wakefieldFamilyResponse = await container.CreateItemAsync(wakefieldFamily, new PartitionKey(wakefieldFamily.PartitionKey));

//                // アイテムを作成した後、ItemResponse の Resource プロパティでアイテムのボディにアクセスできることに注意してください。また、RequestChargeプロパティにアクセスして、このリクエストで消費されたRUの量を確認することもできます。
//                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", wakefieldFamilyResponse.Resource.Id, wakefieldFamilyResponse.RequestCharge);
//            }
//        }

//        /// <summary>
//        /// コンテナに対して（Azure Cosmos DB SQL 構文を使用して）クエリを実行する パーティションキー値 lastName を WHERE フィルタに含めると、より効率的なクエリが実行される。
//        /// </summary>
//        private async Task QueryItemsAsync()
//        {
//            var sqlQueryText = "SELECT * FROM c WHERE c.PartitionKey = 'Andersen'";

//            Console.WriteLine("Running query: {0}\n", sqlQueryText);

//            var queryDefinition = new QueryDefinition(sqlQueryText);    // クエリ定義を作成
//            FeedIterator<Family> queryResultSetIterator = container.GetItemQueryIterator<Family>(queryDefinition);  // 実際に検索

//            List<Family> families = []; // ここに結果を格納

//            while (queryResultSetIterator.HasMoreResults)
//            {
//                FeedResponse<Family> currentResultSet = await queryResultSetIterator.ReadNextAsync();
//                foreach (Family family in currentResultSet)
//                {
//                    families.Add(family);
//                    Console.WriteLine("\tRead {0}\n", family);
//                }
//            }
//        }

//        /// <summary>
//        /// コンテナ内のアイテムを置き換える（更新する）
//        /// </summary>
//        private async Task ReplaceFamilyItemAsync()
//        {
//            ItemResponse<Family> wakefieldFamilyResponse = await container.ReadItemAsync<Family>("Wakefield.7", new PartitionKey("Wakefield"));     // IDとパーティションキーを指定してアイテムを読み取る
//            var itemBody = wakefieldFamilyResponse.Resource;

//            // 登録ステータスをfalseからtrueに更新
//            itemBody.IsRegistered = true;
//            // 子供の学年を更新する
//            itemBody.Children[0].Grade = 6;

//            // 更新された内容で項目を置き換える
//            wakefieldFamilyResponse = await container.ReplaceItemAsync(itemBody, itemBody.Id, new PartitionKey(itemBody.PartitionKey));
//            Console.WriteLine("Updated Family [{0},{1}].\n \tBody is now: {2}\n", itemBody.LastName, itemBody.Id, wakefieldFamilyResponse.Resource);
//        }

//        /// <summary>
//        /// コンテナ内の項目を削除する
//        /// </summary>
//        private async Task DeleteFamilyItemAsync()
//        {
//            var partitionKeyValue = "Wakefield";
//            var familyId = "Wakefield.7";

//            // アイテムを削除する。削除するアイテムのパーティションキーの値とidを指定しなければならないことに注意してください。
//            ItemResponse<Family> wakefieldFamilyResponse = await container.DeleteItemAsync<Family>(familyId, new PartitionKey(partitionKeyValue));
//            Console.WriteLine("Deleted Family [{0},{1}]\n", partitionKeyValue, familyId);
//        }

//        /// <summary>
//        /// データベースを削除し、Cosmosクライアントインスタンスを破棄します。
//        /// </summary>
//        private async Task DeleteDatabaseAndCleanupAsync()
//        {
//            DatabaseResponse databaseResourceResponse = await database.DeleteAsync();
//            // Also valid: await this.cosmosClient.Databases["FamilyDatabase"].DeleteAsync();

//            Console.WriteLine("Deleted Database: {0}\n", databaseId);

//            // CosmosClientの破棄
//            cosmosClient.Dispose();
//        }

//    }
//}
