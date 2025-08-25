using NTIH.Database;
using NTIH.Database.Metadata;
using NTIH.Database.Modeling;

namespace HomeControl.Models.ServicesInterfaces
{
    public interface IDatabaseConnectionService : IDisposable
    {
        public static bool TryGetMetadata(string modelName, out Type modelType, out DatabaseModelMetadata metadata)
        {
            return DatabaseConnection.TryGetMetadata(modelName, out modelType, out metadata);
        }

        public static bool TryGetMetadata<T>(out DatabaseModelMetadata metadata) where T : DatabaseModel
        {
            return DatabaseConnection.TryGetMetadata<T>(out metadata);
        }

        public static bool TryGetMetadata(Type modelType, out DatabaseModelMetadata metadata)
        {
            return DatabaseConnection.TryGetMetadata(modelType, out metadata);
        }

        IQuery Insert<T>(T instance) where T : DatabaseModel;

        ISelectSingle<T> SelectSingle<T>(int id) where T : IdentityKeyModel;

        ISelectSingle<T> SelectSingle<T>(string id) where T : StringKeyModel;

        ISelectMany<T> Select<T>() where T : DatabaseModel;

        IQuery Update<T>(T instance) where T : DatabaseModel;

        IQuery Delete<T>(T instance) where T : DatabaseModel;

        Task CommitTransactionAsync();
    }
}
