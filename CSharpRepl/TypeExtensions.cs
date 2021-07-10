using System;
using System.Linq;

namespace CSDiscordService
{
    public static class TypeExtensions
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
        private const string ArrayBrackets = "[]";

        private static string GetPrimitiveTypeName(Type type)
        {
            var typeName = type.Name;
            if (type.IsArray)
            {
                typeName = typeName.Replace(ArrayBrackets, string.Empty);
            }

            string returnValue;
            switch (typeName)
            {
                case "Boolean": returnValue = "bool"; break;
                case "Byte": returnValue = "byte"; break;
                case "Char": returnValue = "char"; break;
                case "Decimal": returnValue = "decimal"; break;
                case "Double": returnValue = "double"; break;
                case "Int16": returnValue = "short"; break;
                case "Int32": returnValue = "int"; break;
                case "Int64": returnValue = "long"; break;
                case "SByte": returnValue = "sbyte"; break;
                case "Single": returnValue = "float"; break;
                case "String": returnValue = "string"; break;
                case "UInt16": returnValue = "ushort"; break;
                case "UInt32": returnValue = "uint"; break;
                case "UInt64": returnValue = "ulong"; break;
                case "Object": returnValue = "object"; break;
                default:
                    return type.Name;
            }

            if (type.IsArray)
            {
                return string.Join(string.Empty, returnValue, ArrayBrackets);
            }
            return returnValue;
        }
    }
}
