using HomeControl.Models.Modeling;
using HomeControl.Models.ServicesInterfaces;
using Microsoft.Extensions.DependencyInjection;
using NTIH.Database.Modeling;
using NTIH.Database.Modeling.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace HomeControl.Models.DatabaseModels
{
    public abstract class Action : IdentityKeyModel, IIndexedObject, IDisplayable<ActionDisplay>
    {
        [Column]
        public int Index { get => Get<int>(); set { Set(value); } }

        [Column]
        public ActionType Type { get => Get<ActionType>(); set => Set(value); }

        [Column]
        [JsonField]
        public ActionData Data { get => Get<ActionData>(); set => Set(value); }
    }

    public enum ActionType
    {
        [Description("Execute Feature")]
        ExecuteFeature,
        [Description("Schedule Feature Execution")]
        ScheduleFeatureExecution,
        [Description("Clear Devices Cache")]
        ClearIntegrationDevicesCache,
        [Description("Activate Routine")]
        ActivateRoutine,
        [Description("Deactivate Routine")]
        DeactivateRoutine
    }

    public class ActionDisplay : DisplayBase<Action>
    {
        public override async Task Create(Action action, IServiceProvider serviceProvider)
        {
            if (action.Data is IDisplayable displayableData)
            {
                var displayFactory = serviceProvider.GetService<IDisplayFactory>();

                var display = await displayFactory.CreateDisplayAsync(displayableData);
                Display = display.Display;
                AdditionalInfo = display.AdditionalInfo;
            }
        }
    }

    public abstract class ActionData : DatabaseModel, IDisplayable<ActionDataDisplay>
    {
        public virtual async Task<(string display, string additionalInfo)> CreateDisplay(IServiceProvider serviceProvider)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            return (ToString(), null);
        }
    }

    public class ActionDataDisplay : DisplayBase<ActionData>
    {
        public override async Task Create(ActionData actionData, IServiceProvider serviceProvider)
        {
            (Display, AdditionalInfo) = await actionData.CreateDisplay(serviceProvider).ConfigureAwait(false);
        }
    }

    public abstract class DeviceActionData : ActionData
    {
        public int DeviceId { get => Get<int>(); set => Set(value); }

        public override async Task<(string display, string additionalInfo)> CreateDisplay(IServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetService<IDatabaseConnectionService>();
            var deviceService = serviceProvider.GetService<IDeviceService>();
            
            var device = await db.SelectSingle<Device>(DeviceId).ExecuteAsync().ConfigureAwait(false);

            var integrationDevice = await deviceService.CreateAndInitializeIntegrationDeviceAsync(device).ConfigureAwait(false);

            return ($"{integrationDevice.DisplayName}: {ToString()}", null);
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

    public class RoutineActionData : ActionData
    {
        public int RoutineId { get => Get<int>(); set => Set(value); }

        public override async Task<(string display, string additionalInfo)> CreateDisplay(IServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetService<IDatabaseConnectionService>();
            var routine = await db.SelectSingle<Routine>(RoutineId).ExecuteAsync().ConfigureAwait(false);
            return ($"{routine.Name}", null);
        }
    }
}