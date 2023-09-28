using Newtonsoft.Json.Linq;

namespace RecommendProductsCustomers.Common
{
    public class Format
    {
        public static string JObjectToString(JObject pJobject)
        {
            List<string> result = new List<string>();
            string[] split = pJobject.ToString().Split(',');
            foreach (string item in split)
            {
                string[] split2 = item.Split(':');

                split2[0] = split2[0].Replace("\"", "");

                result.Add(split2[0] + ":" + split2[1]);
            }

            return string.Join(",", result);
        }
    }
}
