using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
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

    public class NumberConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => 
            typeToConvert != typeof(float)
            && typeToConvert != typeof(double)
            && typeToConvert.GetInterfaces().Any(i => i.Name == "INumber`1" && i.GenericTypeArguments[0] == typeToConvert);
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter)Activator.CreateInstance(typeof(NumberJsonConverter<>).MakeGenericType(typeToConvert));
    }
    
    public class NumberJsonConverter<T> : JsonConverter<T> where T : INumber<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (T.IsInteger(value))
            {
                var longValue = long.CreateSaturating(value);
                var ulongValue = ulong.CreateSaturating(value);
                if (longValue is not (long.MinValue or long.MaxValue))
                {
                    writer.WriteNumberValue(longValue);
                }
                else if (ulongValue is not ulong.MaxValue)
                {
                    writer.WriteNumberValue(ulongValue);
                }
                else
                {
                    writer.WriteStringValue(value.ToString(null, CultureInfo.InvariantCulture));
                }
            }
            else
            {
                writer.WriteStringValue(value.ToString(null, CultureInfo.InvariantCulture));
            }
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
    
    public class ByteEnumerableJsonConverter : JsonConverter<IEnumerable<byte>>
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert.GetInterfaces().Contains(typeof(IEnumerable<byte>));

        public override IEnumerable<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IEnumerable<byte> value, JsonSerializerOptions options)
        {
            ((JsonConverter<IEnumerable<int>>)options.GetConverter(typeof(IEnumerable<int>))).Write(writer, value.Select(int (x) => x), options);
        }
    }

    public class MultidimArrayConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert is { IsArray: true, IsSZArray: false };
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return (JsonConverter)Activator.CreateInstance(typeof(MultidimArrayJsonConverter<>).MakeGenericType(typeToConvert.GetElementType()!));
        }
    }
    
    public class MultidimArrayJsonConverter<T> : JsonConverter<Array>
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert is { IsArray: true, IsSZArray: false };

        public override Array Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Array value, JsonSerializerOptions options)
        {
            var converter = (JsonConverter<T>)options.GetConverter(typeof(T));
            var currentRankIndexes = Enumerable.Range(0, value.Rank).Select(value.GetLowerBound).ToArray();
            var currentRank = 0;
            while (true)
            {
                if (currentRankIndexes[currentRank] == value.GetLowerBound(currentRank))
                {
                    writer.WriteStartArray();
                }
                if (currentRankIndexes[currentRank] == value.GetUpperBound(currentRank) + 1)
                {
                    writer.WriteEndArray();
                    currentRankIndexes[currentRank] = value.GetLowerBound(currentRank);
                    if (currentRank-- == 0)
                    {
                        return;
                    }
                    else
                    {
                        currentRankIndexes[currentRank] += 1;
                        continue;
                    }
                }
                if (currentRank == currentRankIndexes.Length - 1)
                {
                    converter.Write(writer, (T)value.GetValue(currentRankIndexes), options);
                    currentRankIndexes[currentRank] += 1;
                }
                else
                {
                    currentRank += 1;
                }
            }
        }
    }
}
