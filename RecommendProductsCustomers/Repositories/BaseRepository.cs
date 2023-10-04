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

        #region NODE

        // Create Node
        public async Task<JObject> Add(string pLabel, JObject pJobject)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                string newValue = Format.JObjectToString(pJobject);
                string command = $"create (n:{pLabel} {newValue}) return n";

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

        // Create/Update/Delete properties Node
        public async Task<List<JObject>> Update(string pLabel, JObject pWhere, JObject pNewValue, bool? pUpdate = true)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                string where = Format.JObjectToString(pWhere);

                string command = "";

                if (pUpdate == true)
                {
                    List<string> valueSet = new List<string>();
                    foreach (var item in pNewValue.Properties())
                    {
                        valueSet.Add($"n.{item.Name}=\"{item.Value}\"");
                    }

                    command = $"match (n:{pLabel} {where}) set {string.Join(",", valueSet)} return n";
                }    
                else
                {
                    List<string> propertiesDelete = new List<string>();
                    foreach (var item in pNewValue.Properties())
                    {
                        propertiesDelete.Add($"n.{item.Name}");
                    }

                    command = $"match (n:{pLabel} {where}) remove {string.Join(",", propertiesDelete)} return n";
                }   
                
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

        // Delete Node
        public async Task<int> Delete(string pLabel, JObject pWhere)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                string where = Format.JObjectToString(pWhere);

                string command = $"match (n:{pLabel} {where}) detach delete n return count(n) as deletedCount";

                var commandResult = await tx.RunAsync(command);

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileCommands), command + "\n\n");

                var record = await commandResult.SingleAsync();
                return record["deletedCount"].As<int>();
            });

            return result;
        }

        #endregion


        #region RELATIONSHIP

        // Create RelationShip
        // -1: <-
        //  0:  - (Lỗi)
        //  1: ->
        public async Task<(JObject NodeA, JObject NodeB, JObject Relationship)> CreateRelationShip
                        (string pLabelA, JObject pWhereA, string pLabelB, JObject pWhereB,
                        string pRelationship, JObject propertiesRela, bool? type = true)
        {
            // Lấy điều kiện
            List<string> whereA = new List<string>();
            foreach (var item in pWhereA.Properties())
            {
                whereA.Add($"{item.Name}:\"{item.Value}\"");
            }

            List<string> whereB = new List<string>();
            foreach (var item in pWhereB.Properties())
            {
                whereB.Add($"{item.Name}:\"{item.Value}\"");
            }

            // Lấy hướng
            string direction1 = type == false ? "<-" : "-";
            string direction2 = type == false ? "-" : "->";

            // Thuộc tính của quan hệ
            string properties = propertiesRela == null ? string.Empty : $"{{{Format.JObjectToString(propertiesRela)}}}";

            // Lệnh
            string command = $"match (a:{pLabelA} {{{string.Join(",", whereA)}}}), " +
                                   $"(b:{pLabelB} {{{string.Join(",", whereB)}}}) " +
                             $"create (a){direction1}[r:{pRelationship}{properties}]{direction2}(b) " +
                             $"return a, b, r";

            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                var queryResult = await tx.RunAsync(command);

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileCommands), command + "\n\n");

                var records = await queryResult.ToListAsync();

                if (records.Count < 1)
                {
                    return (null, null, null); // Trả về null nếu không tìm thấy kết quả
                }

                // Trả về a, b và mối quan hệ dưới dạng JObject
                var nodeA = new JObject();
                var nodeB = new JObject();
                var relationship = new JObject();

                foreach (var pair in records[0]["a"].As<INode>().Properties)
                {
                    nodeA.Add(pair.Key, JToken.FromObject(pair.Value));
                }

                foreach (var pair in records[0]["b"].As<INode>().Properties)
                {
                    nodeB.Add(pair.Key, JToken.FromObject(pair.Value));
                }

                foreach (var pair in records[0]["r"].As<IRelationship>().Properties)
                {
                    relationship.Add(pair.Key, JToken.FromObject(pair.Value));
                }

                return (nodeA, nodeB, relationship);
            });

            return result;
        }

        // Create or Update properties RelationShip
        public async Task<List<JObject>> UpdateRelationShip(string pLabelA, JObject pWhereA, string pRelationShip,
            string pLabelB, JObject pWhereB, JObject pNewValue, bool? type = true, bool? pUpdate = true)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                List<string> whereA = new List<string>();
                foreach (var item in pWhereA.Properties())
                {
                    whereA.Add($"{item.Name}:\"{item.Value}\"");
                }

                List<string> whereB = new List<string>();
                foreach (var item in pWhereB.Properties())
                {
                    whereB.Add($"{item.Name}:\"{item.Value}\"");
                }

                // Lấy hướng
                string direction1 = type == false ? "<-" : "-";
                string direction2 = type == false ? "-" : "->";
                string command = "";

                if (pUpdate == true)
                {
                    List<string> valueSet = new List<string>();
                    foreach (var item in pNewValue.Properties())
                    {
                        valueSet.Add($"r.{item.Name}=\"{item.Value}\"");
                    }

                    command = $"match (a:{pLabelA} {{{string.Join(",", whereA)}}}) " +
                              $"{direction1}[r:{pRelationShip}]{direction2} " +
                              $"(b:{pLabelB} {{{string.Join(",", whereB)}}}) " +
                              $"set {string.Join(",", valueSet)} " +
                              $"return a, b, r";
                }
                else
                {
                    List<string> propertiesDelete = new List<string>();
                    foreach (var item in pNewValue.Properties())
                    {
                        propertiesDelete.Add($"r.{item.Name}");
                    }

                    command = $"match (a:{pLabelA} {{{string.Join(",", whereA)}}}) " +
                              $"{direction1}[r:{pRelationShip}]{direction2} " +
                              $"(b:{pLabelB} {{{string.Join(",", whereB)}}}) " +
                              $"remove {string.Join(",", propertiesDelete)} " +
                              $"return a, b, r";
                }

                var commandResult = await tx.RunAsync(command);

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileCommands), command + "\n\n");

                var records = await commandResult.ToListAsync();

                return records.Select(record =>
                {
                    // Chuyển đổi mỗi bản ghi thành một đối tượng JObject
                    var jobject = new JObject();
                    jobject.Add("a", JToken.FromObject(record["a"].As<INode>().Properties));
                    jobject.Add("b", JToken.FromObject(record["b"].As<INode>().Properties));
                    jobject.Add("r", JToken.FromObject(record["r"].As<IRelationship>().Properties));
                    return jobject;
                }).ToList();

                //return records.Select(record =>
                //{
                //    // Chuyển đổi mỗi bản ghi thành một đối tượng JObject
                //    var jobject = new JObject();
                //    foreach (var pair in record["r"].As<INode>().Properties)
                //    {
                //        jobject.Add(pair.Key, JToken.FromObject(pair.Value));
                //    }
                //    return jobject;
                //}).ToList();
            });

            return result;

        }

        // Delete RelationShip
        public async Task<int> DeleteRelationShip(string pLapel, JObject pWhere,
                                                   string pRelationship, bool? type = true)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                // Lấy điều kiện
                List<string> where = new List<string>();
                foreach (var item in pWhere.Properties())
                {
                    where.Add($"{item.Name}:\"{item.Value}\"");
                }

                // Lấy hướng
                string direction1 = type == false ? "<-" : "-";
                string direction2 = type == false ? "-" : "->";

                string command = $"match ({pLapel} {{{string.Join(",", where)}}}) " +
                                 $"{direction1}[r:{pRelationship}]{direction2}() " +
                                 $"delete r " +
                                 $"return count(r) as deletedCount";

                var commandResult = await tx.RunAsync(command);

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileCommands), command + "\n\n");

                var record = await commandResult.SingleAsync();
                return record["deletedCount"].As<int>();
            });

            return result;
        }

        #endregion

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
