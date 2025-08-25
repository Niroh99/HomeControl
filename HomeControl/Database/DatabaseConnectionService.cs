using HomeControl.Models.Modeling;
using HomeControl.Models.ServicesInterfaces;
using NTIH.Database;
using NTIH.Database.Metadata;
using NTIH.Database.Modeling;
using NTIH.Modeling;

namespace HomeControl.Database
{
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
