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

                if (split2[1] == " \"\"")
                {
                    if(split2[0].StartsWith("{"))
                    {
                        result.Add("{");
                    }    
                    continue;
                }    
                if(split2[1] == " \"\"\r\n}")
                {
                    result.Add("}");
                    break;
                }    

                result.Add(split2[0] + ":" + split2[1]);
            }
            if(result[0] == "{")
            {
                result[1] = "{" + result[1];
                result.RemoveAt(0);
            }

            if (result[result.Count - 1] == "}")
            {
                result[result.Count - 2] = result[result.Count - 2] + "}";
                result.RemoveAt(result.Count - 1);

            }
            return string.Join(",", result);
        }
    }
}
