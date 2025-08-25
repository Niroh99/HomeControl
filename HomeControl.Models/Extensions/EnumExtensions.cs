using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.Models.Extensions
{
    public static class EnumExtensions
    {
        public static string GetValueDescription<T>(this T value) where T : Enum
        {
            var type = typeof(T);

            if (!type.IsEnum) throw new InvalidOperationException("value must be of type enum.");

            var memberInfo = type.GetMember(value.ToString()).First();

            var descriptionAttribute = memberInfo.GetCustomAttribute<DescriptionAttribute>();

            if (descriptionAttribute == null) return value.ToString();

            return descriptionAttribute.Description;
        }

        public static string ToShortDayOfWeek(this DayOfWeek value)
        {
            return value switch
            {
                DayOfWeek.Monday => "Mo",
                DayOfWeek.Tuesday => "Tu",
                DayOfWeek.Wednesday => "We",
                DayOfWeek.Thursday => "Th",
                DayOfWeek.Friday => "Fr",
                DayOfWeek.Saturday => "Sa",
                DayOfWeek.Sunday => "Su",
                _ => throw new ArgumentOutOfRangeException(nameof(value)),
            };
        }
    }
}