using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSDiscordService.Infrastructure.JsonFormatters
{
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            TimeSpan.TryParse(reader.GetString(), out var ts);
            return ts;
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
