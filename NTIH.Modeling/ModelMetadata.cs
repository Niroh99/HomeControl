using System.Collections.Generic;

namespace NTIH.Modeling
{
    public class ModelMetadata<T> where T : FieldMetadata
    {
        public List<T> Fields { get; } = new List<T>();
    }
}