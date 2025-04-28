using HomeControl.Database;
using HomeControl.Integrations;
using HomeControl.Modeling;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace HomeControl.DatabaseModels
{
    public abstract class Action : IdentityKeyModel, IIndexedObject
    {
        [Column]
        public int Index { get => Get<int>(); set { Set(value); } }

        [Column]
        public ActionType Type { get => Get<ActionType>(); set => Set(value); }

        [Column]
        [JsonField]
        public Model Data { get => Get<Model>(); set => Set(value); }

        public override async Task<string> ToString(IServiceProvider serviceProvider)
        {
            await Task.CompletedTask;
            return Data.Display;
        }
    }

    public enum ActionType
    {
        [Description("Execute Feature")]
        ExecuteFeature,
        [Description("Schedule Feature Execution")]
        ScheduleFeatureExecution
    }

    public class DeviceActionData : Model
    {
        public int DeviceId { get => Get<int>(); set => Set(value); }

        public override async Task<string> ToString(IServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetService<IDatabaseConnection>();
            var deviceService = serviceProvider.GetService<IDeviceService>();

            var device = await db.SelectSingleAsync<Device>(DeviceId);

            var integrationDevice = await deviceService.CreateAndInitializeIntegrationDeviceAsync(device);

            return $"{integrationDevice.DisplayName}: {ToString()}";
        }
    }

    public class ExecuteDeviceFeatureActionData : DeviceActionData
    {
        public string FeatureName { get => Get<string>(); set => Set(value); }

        public override string ToString()
        {
            return FeatureName;
        }
    }

    public class ScheduleDeviceFeatureExecutionActionData : ExecuteDeviceFeatureActionData
    {
        public int ExecuteIn { get => Get<int>(); set => Set(value); }

        public override string ToString()
        {
            return base.ToString() + $" after {ExecuteIn} min";
        }
    }
}