using NTIH.Database.Modeling.Attributes;
using System.Reflection;

namespace NTIH.Database.Metadata
{
    public class DatabaseField : NTIH.Modeling.FieldMetadata
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