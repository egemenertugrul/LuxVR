using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using System.Text.Json;

namespace Lux.Domain.Internal.Extensions
{
    internal static class JsonExtensions
    {
        public static double GetDoubleOrCoerce(this JObject element)
        {
            // If it's a string, parse
            if (element.Type == JTokenType.String)
                return double.Parse(element.ToString(), CultureInfo.InvariantCulture);

            // Otherwise, try to read as number
            return element.ToObject<double>();
        }
    }
}