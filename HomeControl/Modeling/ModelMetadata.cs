namespace HomeControl.Modeling
{
    public class ModelMetadata<T> where T : FieldMetadata
    {
        public string TableName { get; set; }

        public List<T> Fields { get; } = new List<T>();
    }
}