using Newtonsoft.Json;

namespace Parser
{
    public class SkinParse
    {
        public bool success { get; set; }
        public string lowest_price { get; set; }
        public string volume { get; set; }
        public string median_price { get; set; }
    }
    public class ParserMethods
    {
        private static readonly HttpClient client = new();
        private static readonly List<string> extraChars = new() { "amp;" };
        public static async Task<(DateTime DateUpdate, double Price)> GetPrice(string url)
        {
            string result = client.GetStringAsync($"https://steamcommunity.com/market/priceoverview/?market_hash_name={url[(url.LastIndexOf('/') + 1)..]}&appid=730&currency=5").Result;
            SkinParse skinParse = JsonConvert.DeserializeObject<SkinParse>(result);

            double price = -1;
            try
            {
                price = Convert.ToDouble(skinParse.lowest_price[..^4]);
            }
            catch
            {
                return (DateTime.Now, -1);
            }
            return (DateTime.Now, price);
        }
        public static async Task<string> GetTitle(string url)
        {
            string result = client.GetStringAsync(url).Result;
            string slice = result[result.IndexOf(url)..];
            return DeleteExtraChar(slice[(url.Length + 2)..slice.IndexOf("</a>")]);
        }
        private static string DeleteExtraChar(string title)
        {
            string result = "";
            foreach (string item in extraChars)
            {
                result = title.Replace(item, "");
                title = result;
            }
            return result;
        }
    }
}