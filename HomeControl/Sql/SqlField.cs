using HomeControl.Modeling;

namespace HomeControl.Sql
{
    public class SqlField : FieldMetadata
    {
        public SqlField(string name, string columnName = null) : base(name)
        {
            ColumnName = columnName ?? Name;
        }

        public string ColumnName { get; }
    }
}