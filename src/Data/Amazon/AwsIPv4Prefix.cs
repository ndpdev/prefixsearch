using System.Text.Json.Serialization;
using PrefixSearch.Data.Converters;
using PrefixSearch.Types;

namespace PrefixSearch.Data.Amazon
{
    public class AwsIPv4Prefix
    {
        [JsonPropertyName("ip_prefix")]
        [JsonConverter(typeof(IPv4Converter))]
        public IPv4Prefix Prefix { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("service")]
        public string Service { get; set; }
    }
}