using System;
using System.Reflection;

namespace NTIH.Modeling
{
    public class FieldMetadata
    {
        public FieldMetadata(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            Name = propertyInfo.Name;
        }

        public PropertyInfo PropertyInfo { get; }

        public string Name { get; }

        public object Get(Model model)
        {
            var type = model?.GetType();

            var property = type?.GetProperty(Name);

            var getMethod = property?.GetGetMethod();

            return getMethod?.Invoke(model, Array.Empty<object>());
        }
    }
}