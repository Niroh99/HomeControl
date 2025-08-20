using System.Reflection;

namespace NTIH.Database.Metadata
{
    public class DatabaseNavigationField(PropertyInfo propertyInfo, string foreignKeyFieldName) : DatabaseField(propertyInfo)
    {
        public string ForeignKeyFieldName { get; } = foreignKeyFieldName;
    }
}