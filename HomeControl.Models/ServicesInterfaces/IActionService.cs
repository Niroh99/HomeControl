using HomeControl.Models.DatabaseModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.Models.ServicesInterfaces
{
    public interface IActionsService
    {
        public static ReadOnlyDictionary<ActionType, Type> ActionTypeDataMap { get; } = new Dictionary<ActionType, Type>
        {
            { ActionType.ExecuteFeature, typeof(ExecuteDeviceFeatureActionData) },
            { ActionType.ScheduleFeatureExecution, typeof(ScheduleDeviceFeatureExecutionActionData) },
            { ActionType.ClearIntegrationDevicesCache, typeof(ClearIntegrationDevicesCacheActionData) }
        }.AsReadOnly();

        Task ExecuteActionSequenceAsync<T>(List<T> actions, IServiceProvider serviceProvider) where T : DatabaseModels.Action;
    }
}
