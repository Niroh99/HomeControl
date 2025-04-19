using System.Reflection;

namespace HomeControl.Database
{
    public class PrimaryKeyField(PropertyInfo propertyInfo, string columnName = null) : DatabaseColumnField(propertyInfo, columnName)
    {
        public bool IsIdentity { get; } = propertyInfo.PropertyType == typeof(int);
    }
}