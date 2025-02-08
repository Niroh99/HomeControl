namespace HomeControl.Database
{
    public class PrimaryKeyField : DatabaseField
    {
        public PrimaryKeyField(string name, bool isIdentity, string columnName = null) : base(name, columnName)
        {
            IsIdentity = isIdentity;
        }

        public bool IsIdentity { get; }
    }
}