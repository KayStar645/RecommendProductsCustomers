namespace RecommendProductsCustomers.Common
{
    public static class SettingCommon
    {
        public static string Connect(string pKey)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

            return configuration.GetConnectionString(pKey);
        }
    }
}
