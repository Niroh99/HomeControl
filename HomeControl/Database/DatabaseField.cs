using HomeControl.Modeling;
using System.Reflection;

namespace HomeControl.Database
{
    public class DatabaseField : FieldMetadata
    {
        public DatabaseField(PropertyInfo propertyInfo) : base(propertyInfo)
        {
            var jsonFieldAttribute = propertyInfo.GetCustomAttribute<JsonFieldAttribute>();

            IsJson = jsonFieldAttribute != null;
        }

        public bool IsJson { get; }
    }
}