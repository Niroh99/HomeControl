using System.Runtime.Serialization;

namespace HomeControl.Database
{
    public class UniqueFieldViolationException : Exception
    {
        private const string UniqueFieldViolationMessage = "An Entry with the given Field {0} already exists.";

        public UniqueFieldViolationException(string fieldName) : base(string.Format(UniqueFieldViolationMessage, fieldName))
        {
            FieldName = fieldName;
        }

        public UniqueFieldViolationException(string fieldName, Exception innerException) : base(string.Format(UniqueFieldViolationMessage, fieldName), innerException)
        {
            FieldName = fieldName;
        }

        public string FieldName { get; }
    }
}