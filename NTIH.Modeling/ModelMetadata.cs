using System;
using System.Collections.Generic;

namespace NTIH.Modeling
{
    public class ModelMetadata<T> where T : FieldMetadata
    {
        public ModelMetadata(Type modelType)
        {
            ModelType = modelType;
        }

        public Type ModelType { get; }

        public List<T> Fields { get; } = new List<T>();
    }
}