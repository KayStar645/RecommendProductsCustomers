using Neo4j.Driver;

namespace RecommendProductsCustomers.Example
{
    public class HelloWorld : IDisposable
    {
        private readonly IDriver _driver;

        public HelloWorld(string pUri, string pUserName, string pPassword)
        {
            _driver = GraphDatabase.Driver(pUri, AuthTokens.Basic(pUserName, pPassword));
        }

        public async Task<List<IRecord>> GetPeople()
        {
            using var session = _driver.AsyncSession();

            var peopleList = await session.ExecuteWriteAsync(async tx =>
            {
                var result = await tx.RunAsync("MATCH (n:People) RETURN n LIMIT 25");

                // Tạo một danh sách để lưu trữ các nodes People
                var people = new List<IRecord>();

                // Lặp qua kết quả và thêm các node vào danh sách
                while (await result.FetchAsync())
                {
                    var record = result.Current;
                    people.Add(record);
                }

                return people;
            });

            return peopleList;
        }



        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
