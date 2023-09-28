using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;
using System.Reflection.Emit;
using System.Xml.Linq;
using Neo4j.Driver;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace RecommendProductsCustomers.Repositories
{
    public class BaseRepository : IDisposable
    {
        private readonly IDriver _driver;

        public BaseRepository(string pUri, string pUserName, string pPassword)
        {
            _driver = GraphDatabase.Driver(pUri, AuthTokens.Basic(pUserName, pPassword));
        }

        public async Task<List<JObject>> Get(string pLabel, int? pLimit = 10)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                string query = $"match (n:{pLabel}) return n limit {pLimit}";

                var queryResult = await tx.RunAsync(query);

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileQueries), query + "\n");

                var records = await queryResult.ToListAsync();

                return records.Select(record =>
                {
                    // Chuyển đổi mỗi bản ghi thành một đối tượng JObject
                    var jobject = new JObject();
                    foreach (var pair in record["n"].As<INode>().Properties)
                    {
                        jobject.Add(pair.Key, JToken.FromObject(pair.Value));
                    }
                    return jobject;
                }).ToList();
            });

            return result;
        }

        public async Task<JObject> Add(string pLabel, JObject pJoject)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                string json = Format.JObjectToString(pJoject);
                string command = $"create (n:{pLabel} {json}) return n";

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileCommands), command + "\n");

                var commandResult = await tx.RunAsync(command);

                // Lấy đối tượng vừa thêm từ kết quả trả về
                var record = await commandResult.SingleAsync();

                return record["n"].As<INode>();
            });

            var jobject = new JObject();

            foreach (var pair in result.Properties)
            {
                jobject.Add(pair.Key, JToken.FromObject(pair.Value));
            }

            return jobject;
        }


        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
