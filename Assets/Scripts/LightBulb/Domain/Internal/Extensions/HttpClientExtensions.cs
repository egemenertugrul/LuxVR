using System.Net.Http;
//using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Lux.Domain.Internal.Extensions
{
    internal static class HttpClientExtensions
    {
        public static async Task<T> GetJsonAsync<T>(string url)
        {
            using (var stream = await new HttpClient().GetStreamAsync(url))
            {
                using (var sr = new StreamReader(stream))
                {
                    using (var jr = new JsonTextReader(sr))
                    {
                        return new JsonSerializer().Deserialize<T>(jr);
                    }
                }
            }
        }

        public static async Task<T> GetJsonAsync<T>(this HttpClient httpClient, string uri)
        {
            using (var stream = await httpClient.GetStreamAsync(uri))
            {
                using (var sr = new StreamReader(stream))
                {
                    using (var jr = new JsonTextReader(sr))
                    {
                        return new JsonSerializer().Deserialize<T>(jr);
                    }
                }
            }
        }
    }
}