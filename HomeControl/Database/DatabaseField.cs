using HomeControl.Modeling;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace HomeControl.Database
{
    public class DatabaseField : FieldMetadata
    {
        public DatabaseField(PropertyInfo propertyInfo) : base(propertyInfo)
        {
            var uniqueAttribute = propertyInfo.GetCustomAttribute<UniqueAttribute>();
            var jsonFieldAttribute = propertyInfo.GetCustomAttribute<JsonFieldAttribute>();

            IsUnique = uniqueAttribute != null;
            IsJson = jsonFieldAttribute != null;
        }

        public bool IsUnique { get; }

        public bool IsJson { get; }
    }
}