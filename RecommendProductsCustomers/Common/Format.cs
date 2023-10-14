using Newtonsoft.Json.Linq;

namespace RecommendProductsCustomers.Common
{
    public class Format
    {
        public static string JObjectToString(JObject pJobject)
        {
            string json = "";
            json += "{";
            foreach (var property in pJobject.Properties())
            {
                if(string.IsNullOrEmpty(property.Value.ToString()))
                {
                    continue;
                }

                if (json != "{")
                {
                    json += ",";
                }

                if (property.Name == "images")
                {
                    json += property.Name.Replace("\"", string.Empty);
                    json += ":";

                    string value = property.Value.ToString();

                    json += "\"" + value.Replace("\"", string.Empty) + "\"";
                    continue;
                }   

                json += property.Name.Replace("\"", string.Empty);
                json += ":";
                string value2 = property.Value.ToString().Replace("\"", "'");
                json += $"\"{value2}\""; 
            }
            json += "}";
            return json;
        }


    }
}
