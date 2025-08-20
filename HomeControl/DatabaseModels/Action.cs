using HomeControl.Database;
using HomeControl.Integrations;
using HomeControl.Integrations.TPLink;
using HomeControl.Modeling;
using NTIH.Database.Modeling;
using NTIH.Database.Modeling.Attributes;
using NTIH.Modeling;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace HomeControl.DatabaseModels
{
    public abstract class Action : IdentityKeyModel, IIndexedObject, IDisplayable
    {
        [Column]
        public int Index { get => Get<int>(); set { Set(value); } }

        [Column]
        public ActionType Type { get => Get<ActionType>(); set => Set(value); }

        [Column]
        [JsonField]
        public ActionData Data { get => Get<ActionData>(); set => Set(value); }

        private string _display;
        public string Display => _display;

        private string _additionalInfo;
        public string AdditionalInfo => _additionalInfo;

        public async Task CreateDisplay(IServiceProvider serviceProvider)
        {
            if (Data is IDisplayable displayableData)
            {
                await displayableData.CreateDisplay(serviceProvider);
                _display = displayableData.Display;
                _additionalInfo = displayableData.AdditionalInfo;
            }
        }
    }

    public enum ActionType
    {
        [Description("Execute Feature")]
        ExecuteFeature,
        [Description("Schedule Feature Execution")]
        ScheduleFeatureExecution,
        [Description("Clear Devices Cache")]
        ClearIntegrationDevicesCache
    }

    public abstract class ActionData : Model, IDisplayable
    {
        private string _display;
        public string Display { get; protected set; }

        private string _additionalInfo;
        public string AdditionalInfo => _additionalInfo;

        public virtual async Task CreateDisplay(IServiceProvider serviceProvider)
        {
            _display = ToString();
            await Task.CompletedTask;
        }
    }

    public abstract class DeviceActionData : ActionData
    {
        public int DeviceId { get => Get<int>(); set => Set(value); }

        public override async Task CreateDisplay(IServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetService<IDatabaseConnectionService>();
            var deviceService = serviceProvider.GetService<IDeviceService>();

            var device = await db.SelectSingle<Device>(DeviceId).ExecuteAsync();

            var integrationDevice = await deviceService.CreateAndInitializeIntegrationDeviceAsync(device);

            Display = $"{integrationDevice.DisplayName}: {ToString()}";
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

    public class ClearIntegrationDevicesCacheActionData : ActionData
    {
        public override string ToString()
        {
            return "Clear Devices Cache";
        }
    }
}