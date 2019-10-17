using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using PrefixSearch.Data.Azure;

namespace PrefixSearch.Data.Converters
{
    public class AzureDictionaryServiceTagConverter : JsonConverter<Dictionary<string, ServiceTag>>
    {
        public override Dictionary<string, ServiceTag> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray) { throw new FormatException(); }
            var value = new Dictionary<string, ServiceTag>();

            bool objectInScope = false;
            string id = string.Empty, name = string.Empty;
            ServiceTag tag = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName && objectInScope)
                {
                    string propertyName = reader.GetString();
                    if (propertyName == "id" && reader.Read() && reader.TokenType == JsonTokenType.String) { id = reader.GetString(); }
                    else if (propertyName == "name" && reader.Read() && reader.TokenType == JsonTokenType.String) { name = reader.GetString(); }
                    else if (propertyName == "properties" && reader.Read() && reader.TokenType == JsonTokenType.StartObject)
                    {
                        tag = JsonSerializer.Deserialize<ServiceTag>(ref reader, options);
                    }
                    else { throw new FormatException(); }
                }
                else if (reader.TokenType == JsonTokenType.StartObject && !objectInScope)
                {
                    id = string.Empty;
                    name = string.Empty;
                    tag = null;
                    objectInScope = true;
                }
                else if (reader.TokenType == JsonTokenType.EndObject && objectInScope)
                {
                    tag.Name = name;
                    value.Add(id, tag);
                    objectInScope = false;
                }
                else if (reader.TokenType == JsonTokenType.EndArray) { return value; }
                else { throw new FormatException(); }
            }

            throw new FormatException();
        }
        
        public override void Write(Utf8JsonWriter writer, Dictionary<string, ServiceTag> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var kvp in value)
            {
                writer.WriteStartObject();
                writer.WriteString("id", kvp.Key);
                writer.WriteString("name", kvp.Value.Name);
                writer.WritePropertyName("properties");
                JsonSerializer.Serialize(writer, kvp.Value, options);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
}