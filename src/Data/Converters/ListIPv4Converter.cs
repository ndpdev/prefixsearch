using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using PrefixSearch.Types;

namespace PrefixSearch.Data.Converters
{
    public class ListIPv4Converter : JsonConverter<List<IPv4Prefix>>
    {
        public override List<IPv4Prefix> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray) { throw new FormatException(); }
            var value = new List<IPv4Prefix>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.String) { value.Add(IPv4.ParsePrefix(reader.GetString())); }
                else if (reader.TokenType == JsonTokenType.EndArray) { return value; }
                else { throw new FormatException(); }
            }

            throw new FormatException();
        }
        
        public override void Write(Utf8JsonWriter writer, List<IPv4Prefix> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var prefix in value) { writer.WriteStringValue(prefix.ToString()); }
            writer.WriteEndArray();
        }
    }
}