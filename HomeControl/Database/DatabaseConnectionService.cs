using HomeControl.Modeling;
using NTIH.Database;
using NTIH.Database.Metadata;
using NTIH.Database.Modeling;
using NTIH.Modeling;

namespace HomeControl.Database
{
    public interface IDatabaseConnectionService : IDisposable
    {
        bool TryGetMetadata(string modelName, out Type modelType, out DatabaseModelMetadata metadata);

        bool TryGetMetadata<T>(out DatabaseModelMetadata metadata) where T : DatabaseModel;

        bool TryGetMetadata(Type modelType, out DatabaseModelMetadata metadata);

        InsertQuery<T> Insert<T>(T instance) where T : DatabaseModel;

        SelectSingleIdentityKeyModelQuery<T> SelectSingle<T>(int id) where T : IdentityKeyModel;

        SelectSingleStringKeyModelQuery<T> SelectSingle<T>(string id) where T : StringKeyModel;

        SelectQuery<T> Select<T>() where T : DatabaseModel;

        UpdateQuery<T> Update<T>(T instance) where T : DatabaseModel;

        DeleteQuery<T> Delete<T>(T instance) where T : DatabaseModel;

        Task CommitTransactionAsync();
    }

    public class DatabaseConnectionService(string connectionString, IServiceProvider serviceProvider) : DatabaseConnection(connectionString), IDatabaseConnectionService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public override async Task OnInserted(DatabaseModel model)
        {
            if (model is IDisplayable displayableModel) await displayableModel.CreateDisplay(_serviceProvider);
        }

        public override async Task OnSelected(DatabaseModel model)
        {
            if (model is IDisplayable displayableModel) await displayableModel.CreateDisplay(_serviceProvider);
        }

        public override async Task OnUpdated(DatabaseModel model)
        {
            if (model is IDisplayable displayableModel) await displayableModel.CreateDisplay(_serviceProvider);
        }

        public override async Task<object> DeserializeJsonField(string valueJson)
        {
            var jsonField = await base.DeserializeJsonField(valueJson);

            if (jsonField is IDisplayable displayableJsonField) await displayableJsonField.CreateDisplay(_serviceProvider);

            return jsonField;
        }
    }
}
