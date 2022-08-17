using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brass.Helpers;

public static class Types
{
    public static bool IsPrimitiveType(this Type type)
    {
        return
            type.IsPrimitive ||
            new Type[] {
                typeof(string),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid)
            }.Contains(type) ||
            type.IsEnum ||
            Convert.GetTypeCode(type) != TypeCode.Object ||
            (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsPrimitiveType(type.GetGenericArguments()[0]));
    }
}
