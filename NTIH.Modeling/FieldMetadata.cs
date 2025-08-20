using System.Reflection;

namespace NTIH.Modeling
{
    public class FieldMetadata(PropertyInfo propertyInfo)
    {
        public PropertyInfo PropertyInfo { get; } = propertyInfo;

        public string Name { get; } = propertyInfo.Name;

        public object Get(Model model)
        {
            var type = model.GetType();

            var property = type.GetProperty(Name);

            var getMethod = property.GetGetMethod();

            return getMethod.Invoke(model, []);
        }
    }
}