using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSDiscordService
{
    public static partial class TypeExtensions
    {
        public static string ParseGenericArgs(this Type type)
        {
            var args = type.GetGenericArguments();

            if (args.Length == 0)
            {
                return GetPrimitiveTypeName(type);
            }

            var returnTypeName = type.Name;
            var returnArgs = args.Select(a => a.ParseGenericArgs());
            return returnTypeName.Replace($"`{args.Length}", $"<{string.Join(", ", returnArgs)}>");
        }

        private static string GetPrimitiveTypeName(Type type)
        {
            var typeName = type.Name;
            if (type.IsArray)
            {
                typeName = ParseGenericArgs(type.GetElementType()!);
            }

            var returnValue = typeName switch
            {
                "Boolean" => "bool",
                "Byte" => "byte",
                "Char" => "char",
                "Decimal" => "decimal",
                "Double" => "double",
                "Int16" => "short",
                "Int32" => "int",
                "Int64" => "long",
                "SByte" => "sbyte",
                "Single" => "float",
                "String" => "string",
                "UInt16" => "ushort",
                "UInt32" => "uint",
                "UInt64" => "ulong",
                "Object" => "object",
                "IntPtr" => "nint",
                "UIntPtr" => "nuint",
                _ => typeName
            };

            if (type.IsArray)
            {
                returnValue = $"{returnValue}[{new string(',', type.GetArrayRank() - 1)}]";
            }
            return returnValue;
        }
    }
}
