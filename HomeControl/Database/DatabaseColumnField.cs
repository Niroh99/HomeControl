using System.Reflection;

namespace HomeControl.Database
{
    public class DatabaseColumnField : DatabaseField
    {
        public DatabaseColumnField(PropertyInfo propertyInfo, string columnName = null) : base(propertyInfo)
        {
            ColumnName = columnName ?? Name;
        }

        public string ColumnName { get; }
    }
}