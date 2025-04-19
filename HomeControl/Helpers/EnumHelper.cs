using System.ComponentModel;
using System.Reflection;

namespace HomeControl.Helpers
{
    public static class EnumHelper
    {
        public static string GetValueDescription<T>(T value) where T : Enum
        {
            var type = typeof(T);

            if (!type.IsEnum) throw new InvalidOperationException("value must be of type enum.");
            
            var memberInfo = type.GetMember(value.ToString()).First();

            var descriptionAttribute = memberInfo.GetCustomAttribute<DescriptionAttribute>();

            if (descriptionAttribute == null) return value.ToString();

            return descriptionAttribute.Description;
        }
    }
}