using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PrefixSearch.Data.Converters;

namespace PrefixSearch.Data.Amazon
{
    public class AwsIPRange
    {
        // syncToken is a time value that matches the createDate; not converted here
        // - deserialize: DateTime.UnixEpoch.AddSeconds(double.Parse(syncToken))
        // - serialize: Math.Floor((value - DateTime.UnixEpoch).TotalSeconds
        [JsonPropertyName("syncToken")]
        public string SyncToken { get; set; }

        [JsonPropertyName("createDate")]
        [JsonConverter(typeof(AwsTimeConverter))]
        public DateTime CreationDate { get; set; }

        [JsonPropertyName("prefixes")]
        public List<AwsIPv4Prefix> IPv4Prefixes { get; set; }

        [JsonPropertyName("ipv6_prefixes")]
        public List<AwsIPv6Prefix> IPv6Prefixes { get; set; }
    }
}