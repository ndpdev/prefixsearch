using System.Collections.Generic;
using System.Text.Json.Serialization;
using PrefixSearch.Data.Converters;
using PrefixSearch.Types;

namespace PrefixSearch.Data.Azure
{
    public class ServiceTag
    {
        [JsonPropertyName("changeNumber")]
        public int ChangeNumber { get; set; }

        // Value is inherited from parent on deserialization
        [JsonIgnore()]
        public string Name { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("platform")]
        public string Platform { get; set; }

        [JsonPropertyName("systemService")]
        public string SystemService { get; set; }

        //TODO: Watch for inclusion of IPv6 addresses, which Microsoft has stated intent for
        [JsonPropertyName("addressPrefixes")]
        [JsonConverter(typeof(ListIPv4Converter))]
        public List<IPv4Prefix> AddressPrefixes { get; set; }
    }
}