using System.Text.Json.Serialization;

namespace PrefixSearch.Data.Amazon
{
    public class AwsIPv6Prefix
    {
        //TODO: Add converter for IPv6 type when created
        [JsonPropertyName("ipv6_prefix")]
        public string Prefix { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("service")]
        public string Service { get; set; }
    }
}