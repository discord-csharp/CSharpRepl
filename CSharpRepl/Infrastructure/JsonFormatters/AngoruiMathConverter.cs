using AngouriMath;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSDiscordService.Infrastructure.JsonFormatters
{
    public class AngouriMathEntityConverter : JsonConverter<Entity>
    {
        public override Entity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, Entity value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Stringize());
        }
    }

    public class AngouriMathEntityVarsConverter : JsonConverter<Entity.Variable>
    {
        public override Entity.Variable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, Entity.Variable value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Stringize());
        }
    }
}

