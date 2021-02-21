using System;
using System.Linq;
using System.Reflection;

namespace BenchmarkLogGenerator.Utilities
{
    public static class ExtendedType
    {
        public static FieldInfo[] GetFieldsOrdered(this Type type)
        {
            return type.GetFields().OrderBy(fi => fi.MetadataToken).ToArray();
        }

        public static Type GetTypeWithNullableSupport(this Type type)
        {
            if (type.Name.StartsWith("Nullable"))
            {
                var declaredFields = ((TypeInfo)type).DeclaredFields;
                var valueFieldInfo = declaredFields.Where(fi => string.Equals(fi.Name, "value", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                return (valueFieldInfo != default(FieldInfo)) ? valueFieldInfo.FieldType : type;
            }

            return type;
        }
    }
}
