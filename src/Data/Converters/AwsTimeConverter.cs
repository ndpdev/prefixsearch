using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrefixSearch.Data.Converters
{
    public class AwsTimeConverter : JsonConverter<DateTime>
    {
        static readonly string timeFormat = "yyyy-MM-dd-HH-mm-ss";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => DateTime.ParseExact(reader.GetString(), timeFormat, CultureInfo.InvariantCulture);
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString(timeFormat, CultureInfo.InvariantCulture));
    }
}