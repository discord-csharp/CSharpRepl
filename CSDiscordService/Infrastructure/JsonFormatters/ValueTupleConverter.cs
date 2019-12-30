using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSDiscordService.Infrastructure.JsonFormatters
{
    public class ValueTupleConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert.Namespace == nameof(System)
            && typeToConvert.IsValueType
            && typeToConvert.Name.StartsWith("ValueTuple")
            && typeof(ITuple).IsAssignableFrom(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => new ValueTupleConverter();

        private class ValueTupleConverter : JsonConverter<object>
        {
            public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => throw new NotSupportedException();

            public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
            {
                var tuple = (ITuple)value;

                writer.WriteStartObject();

                for (var i = 0; i < tuple.Length; i++)
                {
                    writer.WritePropertyName($"Item{i + 1}");
                    JsonSerializer.Serialize(writer, tuple[i], options);
                }

                writer.WriteEndObject();
            }
        }
    }
}
