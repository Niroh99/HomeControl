using System.Reflection;

namespace HomeControl.Database
{
    public class DatabaseNavigationField(PropertyInfo propertyInfo, string foreignKeyFieldName) : DatabaseField(propertyInfo)
    {
        public string ForeignKeyFieldName { get; } = foreignKeyFieldName;
    }
}