namespace HomeControl.Modeling
{
    public class FieldMetadata(string name, Type type)
    {
        public Type Type { get; } = type;

        public string Name { get; } = name;

        public object Get(Model model)
        {
            var type = model.GetType();

            var property = type.GetProperty(Name);

            var getMethod = property.GetGetMethod();

            return getMethod.Invoke(model, []);
        }
    }
}