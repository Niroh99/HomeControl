using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NTIH.Modeling
{
    public static class ReflectionExtensions
    {
        public static bool TryGetCustomAttribute<TAttribute>(this Type type, out TAttribute attribute) where TAttribute : Attribute
        {
            attribute = type.GetCustomAttribute<TAttribute>();
            return attribute != null;
        }

        public static bool TryGetCustomAttribute<TAttribute>(this PropertyInfo propertyInfo, out TAttribute attribute) where TAttribute : Attribute
        {
            attribute = propertyInfo.GetCustomAttribute<TAttribute>();
            return attribute != null;
        }
    }
}
