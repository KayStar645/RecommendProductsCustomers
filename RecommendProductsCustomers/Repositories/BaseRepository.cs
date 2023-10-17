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

        public int CalculateTotalPages(int totalItems, int itemsPerPage)
        {
            int totalPages = (totalItems + itemsPerPage - 1) / itemsPerPage;
            return totalPages;
        }


        public async Task<List<JObject>> Get(string? pLabel = "", JObject? pWhere = null, string? pRelationShip = "", string? pLabelB = "", JObject? pWhereB = null, int? pLimit = 100, string? pKeyword = "", int? pPage = 1)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                string where = pWhere == null ? "" : Format.JObjectToString(pWhere);
                string whereB = pWhereB == null ? "" : Format.JObjectToString(pWhereB);

                // Tính toán offset từ trang và giới hạn
                int offset = ((pPage - 1) * pLimit) ?? 0;

                string query = "";
                string search = "";
                if(search != "")
                {
                    search = $"where any(prop in keys(a) where a[prop] CONTAINS {pKeyword}) ";
                }    

                if (string.IsNullOrEmpty(pRelationShip) == false)
                {
                    query = $"match (a:{pLabel} {where}) " +
                            $"-[:{pRelationShip}]-" +
                            $"(b:{pLabelB} {whereB}) " + search +
                            $"return a skip {offset} limit {pLimit}";
                }
                else
                {
                    query = $"match (a:{pLabel} {where}) " + search +
                            $"return a skip {offset} limit {pLimit}";
                }

                var queryResult = await tx.RunAsync(query, new { keyword = pKeyword });

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileQueries), query + "\n\n");

                var records = await queryResult.ToListAsync();

                return records.Select(record =>
                {
                    // Chuyển đổi mỗi bản ghi thành một đối tượng JObject
                    var jobject = new JObject();
                    jobject.Add("id", JToken.FromObject(record["a"].As<INode>().Id.ToString()));
                    foreach (var pair in record["a"].As<INode>().Properties)
                    {
                        jobject.Add(pair.Key, JToken.FromObject(pair.Value));
                    }
                    return jobject;
                }).ToList();
            });

            return result;
        }


        //public async Task<List<JObject>> Get(string? pLabel = "", JObject? pWhere = null, string? pRelationShip = "", string? pLabelB = "", JObject? pWhereB = null, int? pLimit = 100)
        //{
        //    using var session = _driver.AsyncSession();

        //    var result = await session.ExecuteWriteAsync(async tx =>
        //    {
        //        string where = pWhere == null ? "" : Format.JObjectToString(pWhere);
        //        string whereB = pWhereB == null ? "" : Format.JObjectToString(pWhereB);

        //        string query = "";
        //        if (string.IsNullOrEmpty(pRelationShip) == false)
        //        {
        //            query = $"match (a:{pLabel} {where}) " +
        //                       $"-[:{pRelationShip}]-" +
        //                       $"(b:{pLabelB} {whereB}) " +
        //                       $"return a limit {pLimit}";
        //        }
        //        else
        //        {
        //            query = $"match (a:{pLabel} {where}) " +
        //                       $"return a limit {pLimit}";
        //        }

        //        var queryResult = await tx.RunAsync(query);

        //        File.AppendAllText(SettingCommon.Connect(FileCommon.FileQueries), query + "\n\n");

        //        var records = await queryResult.ToListAsync();

        //        return records.Select(record =>
        //        {
        //            // Chuyển đổi mỗi bản ghi thành một đối tượng JObject
        //            var jobject = new JObject();
        //            jobject.Add("id", JToken.FromObject(record["a"].As<INode>().Id.ToString()));
        //            foreach (var pair in record["a"].As<INode>().Properties)
        //            {
        //                jobject.Add(pair.Key, JToken.FromObject(pair.Value));
        //            }
        //            return jobject;
        //        }).ToList();
        //    });

        //    return result;
        //}

        public async Task<List<IRecord>> GetQuery(string query)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                var queryResult = await tx.RunAsync(query);

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileQueries), query + "\n\n");

                var records = await queryResult.ToListAsync();

                return records;
            });

            return result;
        }

        public async Task<List<Tuple<JObject, JObject, JObject>>> GetHasRela(string pLabelA, string  pIdentity, string pRelationShip,
                                                                   string pLabelB, JObject pWhereB, int? pLimit = 100)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                string whereB = pWhereB == null ? "" : Format.JObjectToString(pWhereB);

                string query = $"MATCH (a:{pLabelA} " +
                                $"-[:{pRelationShip}]->" +
                                $"(b:{pLabelB} {whereB}) " +
                                $"where id(a) = {pIdentity} " +
                                $"RETURN a, b, r LIMIT {pLimit}";

                var queryResult = await tx.RunAsync(query);

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileQueries), query + "\n\n");

                var records = await queryResult.ToListAsync();

                var resultList = new List<Tuple<JObject, JObject, JObject>>();

                foreach (var record in records)
                {
                    var a = record["a"].As<INode>();
                    var b = record["b"].As<INode>();
                    var r = record["r"];

                    var aJObject = new JObject();
                    aJObject.Add("id", JToken.FromObject(a.Id.ToString()));
                    foreach (var pair in a.Properties)
                    {
                        aJObject.Add(pair.Key, JToken.FromObject(pair.Value));
                    }

                    var bJObject = new JObject();
                    bJObject.Add("id", JToken.FromObject(b.Id.ToString()));
                    foreach (var pair in b.Properties)
                    {
                        bJObject.Add(pair.Key, JToken.FromObject(pair.Value));
                    }

                    var rJObject = new JObject();
                    if (r != null)
                    {
                        rJObject.Add("type", JToken.FromObject(r.GetType));
                    }
                    else
                    {
                        rJObject.Add("type", JToken.FromObject("null"));
                    }

                    resultList.Add(Tuple.Create(aJObject, bJObject, rJObject));
                }

                return resultList;
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
                string command = $"create (n:{pLabel} {newValue}) " +
                                 $"return n";

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileCommands), command + "\n\n");

                var commandResult = await tx.RunAsync(command);

                // Lấy đối tượng vừa thêm từ kết quả trả về
                var record = await commandResult.SingleAsync();

                return record["n"].As<INode>();
            });

            var jobject = new JObject();
            jobject.Add("id", result.Id.ToString());
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
                        if(string.IsNullOrEmpty(item.Value.ToString()))
                        {
                            continue;
                        }    
                        valueSet.Add($"n.{item.Name}=\"{item.Value}\"");
                    }

                    command = $"match (n:{pLabel} {where}) " +
                              $"set {string.Join(",", valueSet)} " +
                              $"return n";
                }    
                else
                {
                    List<string> propertiesDelete = new List<string>();
                    foreach (var item in pNewValue.Properties())
                    {
                        propertiesDelete.Add($"n.{item.Name}");
                    }

                    command = $"match (n:{pLabel} {where}) " +
                              $"remove {string.Join(",", propertiesDelete)} " +
                              $"return n";
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

        public async Task<List<JObject>> Update(string pLabel, string pIdentity, JObject pNewValue, bool? pUpdate = true)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                string command = "";

                if (pUpdate == true)
                {
                    List<string> valueSet = new List<string>();
                    foreach (var item in pNewValue.Properties())
                    {
                        if (string.IsNullOrEmpty(item.Value.ToString()))
                        {
                            continue;
                        }
                        valueSet.Add($"n.{item.Name}=\"{item.Value}\"");
                    }

                    command = $"match (n:{pLabel}) " +
                              $"where id(n)={pIdentity} " +
                              $"set {string.Join(",", valueSet)} " +
                              $"return n";
                }
                else
                {
                    List<string> propertiesDelete = new List<string>();
                    foreach (var item in pNewValue.Properties())
                    {
                        propertiesDelete.Add($"n.{item.Name}");
                    }

                    command = $"match (n:{pLabel}) " +
                              $"where id(n)={pIdentity} " +
                              $"remove {string.Join(",", propertiesDelete)} " +
                              $"return n";
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

                string command = $"match (n:{pLabel} {where}) " +
                                 $"detach delete n " +
                                 $"return count(n) as deletedCount";

                var commandResult = await tx.RunAsync(command);

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileCommands), command + "\n\n");

                var record = await commandResult.SingleAsync();
                return record["deletedCount"].As<int>();
            });

            return result;
        }

        //public async Task<int> DeleteNotExists(string pLabel, string pProperties, List<string> pValue)
        //{
        //    using var session = _driver.AsyncSession();

        //    var result = await session.ExecuteWriteAsync(async tx =>
        //    {
        //        string command = $"match (n:{pLabel}) " +
        //                         $"where not exists(n.{pProperties}) " +
        //                         $"or not n.{pProperties} in [{string.Join(", ", pValue.Select(s => $"\"{s}\""))}]" +
        //                         $"detach delete n " +
        //                         $"return count(n) as deletedCount";

        //        var commandResult = await tx.RunAsync(command);

        //        File.AppendAllText(SettingCommon.Connect(FileCommon.FileCommands), command + "\n\n");

        //        var record = await commandResult.SingleAsync();
        //        return record["deletedCount"].As<int>();
        //    });

        //    return result;
        //}

        public async Task<int> Delete(string pLabel, string pIdentity)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {

                string command = $"match (n:{pLabel}) " +
                                 $"where id(n)={pIdentity} " +
                                 $"detach delete n " +
                                 $"return count(n) as deletedCount";

                var commandResult = await tx.RunAsync(command);

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileCommands), command + "\n\n");

                var record = await commandResult.SingleAsync();
                return record["deletedCount"].As<int>();
            });

            return result;
        }



        #endregion


        #region RELATIONSHIP
        // Get
        public async Task<JObject> GetRelationShip(string pLabelA, string pIdentity, string pRala, string pLabelB, JObject pWhereB, bool? type = true)
        {
            // Lấy hướng
            string direction1 = type == false ? "<-" : "-";
            string direction2 = type == false ? "-" : "->";

            List<string> whereB = new List<string>();
            foreach (var item in pWhereB.Properties())
            {
                if (string.IsNullOrEmpty(item.Value.ToString()))
                {
                    continue;
                }
                whereB.Add($"b.{item.Name}=\"{item.Value}\"");
            }

            // Lệnh
            string command = $"MATCH (a:{pLabelA})-[r:{pRala}]->(b:{pLabelB}) " +
                             $"WHERE id(a) = {pIdentity} AND {string.Join(",", whereB)} " +
                             $"RETURN r";

            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                var queryResult = await tx.RunAsync(command);

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileCommands), command + "\n\n");

                var records = await queryResult.ToListAsync();

                if (records.Count < 1)
                {
                    return null;
                }

                var relationship = new JObject();

                foreach (var pair in records[0]["r"].As<IRelationship>().Properties)
                {
                    relationship.Add(pair.Key, JToken.FromObject(pair.Value));
                }

                return relationship;
            });

            return result;
        }

        // Create RelationShip
        // -1: <-
        //  0:  - (Lỗi)
        //  1: ->
        public async Task<(JObject NodeA, JObject NodeB, JObject Relationship)> CreateRelationShip
                        (string pLabelA, JObject pWhereA, string pLabelB, JObject pWhereB,
                        string pRelationship, JObject? propertiesRela = null, bool? type = true)
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
            string properties = propertiesRela == null ? string.Empty : $"{Format.JObjectToString(propertiesRela)}";

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

        public async Task<int> DeleteRelationShipNotExists(string pLapelA, string pIdentity, string pLapelB, string pRela,
                                            string pProperties, List<string> pValue, bool? type = true)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                // Lấy hướng
                string direction1 = type == false ? "<-" : "-";
                string direction2 = type == false ? "-" : "->";

                string command = $"MATCH (a:{pLapelA}){direction1}[r:{pRela}]{direction2}(b:{pLapelB}) " +
                                 $"WHERE id(a)={pIdentity} AND " +
                                 $"NOT b.{pProperties} IN [{string.Join(", ", pValue.Select(s => $"\"{s}\""))}] " +
                                 $"DELETE r " +
                                 $"return count(r) as deletedCount";

                var commandResult = await tx.RunAsync(command);

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileCommands), command + "\n\n");

                var record = await commandResult.SingleAsync();
                return record["deletedCount"].As<int>();
            });

            return result;
        }

        public async Task<string> Max(string pLabel, string property)
        {
            using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {

                string command = $"match (a:{pLabel}) " +
                                 $"with a " +
                                 $"return max(a.{property}) as max";

                var commandResult = await tx.RunAsync(command);

                File.AppendAllText(SettingCommon.Connect(FileCommon.FileQueries), command + "\n\n");

                var record = await commandResult.SingleAsync();
                return record["max"].As<string>();
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
