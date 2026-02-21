namespace NTIH.Database.Metadata
{
    public class DatabaseTableModelMetadata(string tableName, Type modelType) : DatabaseModelMetadata(modelType)
    {
        public string TableName { get; set; } = tableName;
    }
}