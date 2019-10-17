using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PrefixSearch.Types;

namespace PrefixSearch.Data.Converters
{
    public class IPv4Converter : JsonConverter<IPv4Prefix>
    {
        public override IPv4Prefix Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => IPv4Prefix.Parse(reader.GetString());
        public override void Write(Utf8JsonWriter writer, IPv4Prefix value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
    }
}