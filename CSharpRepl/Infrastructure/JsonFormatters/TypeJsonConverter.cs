using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSDiscordService.Infrastructure.JsonFormatters
{
    public class TypeJsonConverter : JsonConverter<Type>
    {
        public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Type.GetType(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.AssemblyQualifiedName);
        }
    }

    public class RuntimeTypeJsonConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Type.GetType(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            writer.WriteStringValue((value as Type).AssemblyQualifiedName);
        }
    }
    
    public class IntPtrJsonConverter : JsonConverter<IntPtr>
    {
        public override IntPtr Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new IntPtr(reader.GetInt64());
        }

        public override void Write(Utf8JsonWriter writer, IntPtr value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.ToInt64());
        }
    }

    public class TypeInfoJsonConverter : JsonConverter<TypeInfo>
    {
        public override TypeInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (TypeInfo)Type.GetType(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, TypeInfo value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.AssemblyQualifiedName);
        }
    }

    public class AssemblyJsonConverter : JsonConverter<Assembly>
    {
        public override Assembly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return default;
        }

        public override void Write(Utf8JsonWriter writer, Assembly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.FullName);
        }
    }

    public class RuntimeAssemblyJsonConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return default;
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            writer.WriteStringValue((value as Assembly).FullName);
        }
    }

    public class ModuleJsonConverter : JsonConverter<Module>
    {
        public override Module Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return default;
        }

        public override void Write(Utf8JsonWriter writer, Module value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.FullyQualifiedName);
        }
    }

    public class RuntimeTypeHandleJsonConverter : JsonConverter<RuntimeTypeHandle>
    {
        public override RuntimeTypeHandle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return default;
        }

        public override void Write(Utf8JsonWriter writer, RuntimeTypeHandle value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Value.ToInt32());
        }
    }
    
    public class TypeJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert == Type.GetType("System.RuntimeType");
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new RuntimeTypeJsonConverter();
        }
    }

    public class AssemblyJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert == Type.GetType("System.Reflection.RuntimeAssembly");
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new RuntimeAssemblyJsonConverter();
        }
    }

    public class DirectoryInfoJsonConverter : JsonConverter<DirectoryInfo>
    {
        public override DirectoryInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new DirectoryInfo(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DirectoryInfo value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.FullName);
        }
    }
}
