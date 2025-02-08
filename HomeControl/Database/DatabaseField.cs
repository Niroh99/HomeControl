using HomeControl.Modeling;

namespace HomeControl.Database
{
    public class DatabaseField : FieldMetadata
    {
        public DatabaseField(string name, string columnName = null) : base(name)
        {
            ColumnName = columnName ?? Name;
        }

        public string ColumnName { get; }
    }
}