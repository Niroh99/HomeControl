using NTIH.Database;
using NTIH.Database.Metadata;
using NTIH.Database.Modeling;

namespace HomeControl.Models.ServicesInterfaces
{
    public interface IDatabaseConnectionService : IDisposable
    {
        public static bool TryGetTableModelMetadata(string modelName, out Type modelType, out DatabaseTableModelMetadata metadata)
        {
            return DatabaseConnection.TryGetTableModelMetadata(modelName, out modelType, out metadata);
        }

        public static bool TryGetTableModelMetadata<T>(out DatabaseTableModelMetadata metadata) where T : DatabaseTableModel
        {
            return DatabaseConnection.TryGetTableModelMetadata<T>(out metadata);
        }

        public static bool TryGetTableModelMetadata(Type modelType, out DatabaseTableModelMetadata metadata)
        {
            return DatabaseConnection.TryGetTableModelMetadata(modelType, out metadata);
        }

        IQuery Insert<T>(T instance) where T : DatabaseTableModel;

        ISelectSingle<T> SelectSingle<T>(int id) where T : IdentityKeyModel;

        ISelectSingle<T> SelectSingle<T>(string id) where T : StringKeyModel;

        ISelectMany<T> Select<T>() where T : DatabaseTableModel;

        IQuery Update<T>(T instance) where T : DatabaseTableModel;

        IQuery Delete<T>(T instance) where T : DatabaseTableModel;

        Task CommitTransactionAsync();
    }
}
