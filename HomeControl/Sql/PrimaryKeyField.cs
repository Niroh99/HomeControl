namespace HomeControl.Sql
{
    public class PrimaryKeyField : SqlField
    {
        public PrimaryKeyField(string name, bool isIdentity, string columnName = null) : base(name, columnName)
        {
            IsIdentity = isIdentity;
        }

        public bool IsIdentity { get; }
    }
}