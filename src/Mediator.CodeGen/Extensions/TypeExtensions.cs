using System;
using System.Text;

namespace Mediator.CodeGen.Extensions
{
    internal static class TypeExtensions
    {
        public static string GetFriendlyName(this Type type)
        {
            if (type == null)
            {
                return string.Empty;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return $"{Nullable.GetUnderlyingType(type)!.GetFriendlyName()}?";
            }

            if (type.IsArray)
            {
                return $"{type.GetElementType()!.GetFriendlyName()}[]";
            }

            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments();

                var stringBuilder = new StringBuilder();

                stringBuilder.Append(type.Name.Split('`')[0]);

                stringBuilder.Append('<');

                for (int i = 0; i < genericArguments.Length; i++)
                {
                    stringBuilder.Append(genericArguments[i].GetFriendlyName());
                    if (i < genericArguments.Length - 1)
                    {
                        stringBuilder.Append(", ");
                    }
                }

                stringBuilder.Append('>');

                return stringBuilder.ToString();
            }

            return type.Name;
        }
    }
}