using HomeControl.Modeling;

namespace HomeControl.Database
{
    public class DatabaseField : FieldMetadata
    {
        public DatabaseField(string name, Type type, string columnName = null) : base(name, type)
        {
            ColumnName = columnName ?? Name;
        }

        public string ColumnName { get; }
    }
}