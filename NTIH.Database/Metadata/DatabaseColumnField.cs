using NTIH.Database.Modeling.Attributes;
using NTIH.Modeling;
using System.Reflection;

namespace NTIH.Database.Metadata
{
    public class DatabaseColumnField : DatabaseField
    {
        public DatabaseColumnField(PropertyInfo propertyInfo, string columnName = null) : base(propertyInfo)
        {
            ColumnName = columnName ?? Name;

            var uniqueAttribute = propertyInfo.GetCustomAttribute<UniqueAttribute>();
            var jsonFieldAttribute = propertyInfo.GetCustomAttribute<JsonFieldAttribute>();

            IsUnique = uniqueAttribute != null;
            IsJson = jsonFieldAttribute != null;
        }

        public string ColumnName { get; }

        public bool IsUnique { get; }

        public bool IsJson { get; }
    }
}