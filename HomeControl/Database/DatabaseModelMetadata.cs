using HomeControl.Modeling;

namespace HomeControl.Database
{
    public class DatabaseModelMetadata : ModelMetadata<DatabaseField>
    {
        public string TableName { get; set; }
    }
}