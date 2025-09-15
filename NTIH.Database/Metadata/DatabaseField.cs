using NTIH.Database.Modeling.Attributes;
using NTIH.Modeling;
using System.Reflection;

namespace NTIH.Database.Metadata
{
    public class DatabaseField(PropertyInfo propertyInfo) : FieldMetadata(propertyInfo)
    {
    }
}