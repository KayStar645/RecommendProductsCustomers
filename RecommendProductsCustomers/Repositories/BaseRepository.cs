using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using RecommendProductsCustomers.Common;

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

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileQueries), query + "\n\n");

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

        // Thêm nút
        public async Task<JObject> Add(string pLabel, JObject pJobject)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                string json = Format.JObjectToString(pJobject);
                string command = $"create (n:{pLabel} {json}) return n";

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileCommands), command + "\n\n" +
                    "");

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

        // Thêm/Sửa thuộc tính nút
        public async Task<List<JObject>> Update(string pLabel, JObject pWhere, JObject pNewValue)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                string where = Format.JObjectToString(pWhere);
                List<string> valueSet = new List<string>();
                foreach (var item in pNewValue.Properties())
                {
                    valueSet.Add($"n.{item.Name}=\"{item.Value}\"");
                }

                string command = $"match (n {where}) set {string.Join(",", valueSet)} return n";

                var commandResult = await tx.RunAsync(command);

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileCommands), command + "\n\n");

                var records = await commandResult.ToListAsync();

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


        // Xóa nút
        public async Task<int> Delete(string pLabel, JObject pJobject)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                string json = Format.JObjectToString(pJobject);

                string command = $"match (n:{pLabel} {json}) delete n return count(n) as deletedCount";

                var commandResult = await tx.RunAsync(command);

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileCommands), command + "\n\n");

                var record = await commandResult.SingleAsync();
                return record["deletedCount"].As<int>();
            });

            return result;
        }



        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
