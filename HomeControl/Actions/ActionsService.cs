using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Events;
using HomeControl.Events.EventDatas;
using HomeControl.Integrations;
using System.Collections.ObjectModel;

namespace HomeControl.Actions
{
    public interface IActionsService
    {
        public static ReadOnlyDictionary<ActionType, Type> ActionTypeDataMap { get; } = new Dictionary<ActionType, Type>
        {
            { ActionType.ExecuteFeature, typeof(ExecuteDeviceFeatureActionData) },
            { ActionType.ScheduleFeatureExecution, typeof(ScheduleDeviceFeatureExecutionActionData) }
        }.AsReadOnly();

        Task ExecuteActionSequenceAsync<T>(List<T> actions, IServiceProvider serviceProvider) where T : DatabaseModels.Action;
    }

    public class ActionsService(IDatabaseConnection db) : IActionsService
    {
        public async Task ExecuteActionSequenceAsync<T>(List<T> actions, IServiceProvider serviceProvider) where T : DatabaseModels.Action
        {
            var deviceService = serviceProvider.GetService<IDeviceService>();
            var eventService = serviceProvider.GetService<IEventService>();

            foreach (var action in actions.OrderBy(action => action.Index))
            {
                switch (action.Type)
                {
                    case ActionType.ExecuteFeature:
                        var executeFeatureDeviceOptionActionData = (ExecuteDeviceFeatureActionData)action.Data;

                        await deviceService.ExecuteFeatureAsync(executeFeatureDeviceOptionActionData.DeviceId, executeFeatureDeviceOptionActionData.FeatureName);
                        break;
                    case ActionType.ScheduleFeatureExecution:
                        var scheduleFeatureExecutionDeviceOptionActionData = (ScheduleDeviceFeatureExecutionActionData)action.Data;

                        await eventService.ScheduleEventAsync(db, EventType.ExecuteDeviceFeature, new ExecuteDeviceFeatureEventData
                        {
                            DeviceId = scheduleFeatureExecutionDeviceOptionActionData.DeviceId,
                            FeatureName = scheduleFeatureExecutionDeviceOptionActionData.FeatureName,
                        }, DateTime.Now.AddMinutes(scheduleFeatureExecutionDeviceOptionActionData.ExecuteIn));
                        break;
                    default: throw new NotImplementedException();
                }
            }
        }
    }
}