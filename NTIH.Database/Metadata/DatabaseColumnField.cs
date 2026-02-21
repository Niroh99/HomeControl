using System.Drawing;
using System.Reflection;
using NTIH.Database.Modeling;
using NTIH.Database.Modeling.Attributes;
using NTIH.Modeling;

namespace NTIH.Database.Metadata
{
    public class DatabaseColumnField : DatabaseField
    {
        public DatabaseColumnField(PropertyInfo propertyInfo, string columnName, ColumnSize size) : base(propertyInfo)
        {
            ColumnName = columnName ?? Name;
            Size = size;

            var uniqueAttribute = propertyInfo.GetCustomAttribute<UniqueAttribute>();
            var jsonFieldAttribute = propertyInfo.GetCustomAttribute<JsonFieldAttribute>();

            IsUnique = uniqueAttribute != null;
            IsJson = jsonFieldAttribute != null;
        }

        public override int Priority => 1;

        public string ColumnName { get; }

        public ColumnSize Size { get; init; }

        public bool IsUnique { get; }

        public bool IsJson { get; }
    }
}