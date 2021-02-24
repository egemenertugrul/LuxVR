using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Lux.Domain.Internal;
using Lux.Domain.Internal.Extensions;
using Newtonsoft.Json.Linq;

namespace Lux.Domain
{
    public class GeoLocationProvider
    {
        private readonly HttpClient _httpClient;

        public GeoLocationProvider(HttpClient httpClient) => _httpClient = httpClient;

        public GeoLocationProvider()
            : this(Http.Client) {}

        public async Task<GeoLocation> GetLocationAsync()
        {
            const string url = "http://ip-api.com/json";
            var json = await _httpClient.GetJsonAsync<JObject>(url);

            var latitude = json["lat"].ToObject<double>();
            var longitude = json["lon"].ToObject<double>();

            return new GeoLocation(latitude, longitude);
        }

        public async Task<GeoLocation> GetLocationAsync(string query)
        {
            var queryEncoded = WebUtility.UrlEncode(query);

            var url = $"https://nominatim.openstreetmap.org/search?q={queryEncoded}&format=json";
            var json = await _httpClient.GetJsonAsync<JObject>(url);

            var firstLocationJson = json.First; // TODO: check

            var latitude = firstLocationJson["lat"].ToObject<double>(); // .GetDoubleOrCoerce();
            var longitude = firstLocationJson["lon"].ToObject<double>();

            return new GeoLocation(latitude, longitude);
        }
    }
}