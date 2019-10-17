using System.Collections.Generic;
using System.Text.Json.Serialization;
using PrefixSearch.Data.Converters;

namespace PrefixSearch.Data.Azure
{
    public class AzureIPRange
    {
        [JsonPropertyName("changeNumber")]
        public int ChangeNumber { get; set; }

        [JsonPropertyName("cloud")]
        public string Cloud { get; set; }

        // Values follows a {"name":string, "id":string, "properties":object} format
        // Treat "id" as dictionary key values, and the combination of "name" and "properties" child object as a ServiceTag
        [JsonPropertyName("values")]
        [JsonConverter(typeof(AzureDictionaryServiceTagConverter))]
        public Dictionary<string, ServiceTag> Services { get; set; }
    }
}