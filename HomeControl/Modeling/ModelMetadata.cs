namespace HomeControl.Modeling
{
    public class ModelMetadata<T> where T : FieldMetadata
    {
        public List<T> Fields { get; } = [];
    }
}