using System.Reflection;
using NTIH.Database.Modeling;

namespace NTIH.Database.Metadata
{
    public class PrimaryKeyField(PropertyInfo propertyInfo, string columnName, ColumnSize size) : DatabaseColumnField(propertyInfo, columnName, size)
    {
        public override int Priority => 0;

        public bool IsIdentity { get; } = propertyInfo.PropertyType == typeof(int);
    }
}