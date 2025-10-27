using NTIH.Database.Modeling.Attributes;
using NTIH.Modeling;
using System.Reflection;

namespace NTIH.Database.Metadata
{
    public abstract class DatabaseField(PropertyInfo propertyInfo) : FieldMetadata(propertyInfo)
    {
        public abstract int Priority { get; }
    }
}