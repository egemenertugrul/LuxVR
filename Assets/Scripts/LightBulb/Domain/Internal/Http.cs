using System.Net.Http;
using System.Net.Http.Headers;

namespace Lux.Domain.Internal
{
    internal static class Http
    {
        public static HttpClient Client { get; } = new HttpClient
        {
            DefaultRequestHeaders =
            {
                // Required by some of the services we're using
                UserAgent =
                {
                    new ProductInfoHeaderValue(
                        "EyeLight",
                        typeof(Http).Assembly.GetName().Version?.ToString()
                    )
                }
            }
        };
    }
}