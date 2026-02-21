using HomeControl.Database;
using HomeControl.Events;
using HomeControl.Integrations;
using HomeControl.Models.DatabaseModels;
using HomeControl.Models.ServicesInterfaces;
using System.Collections.ObjectModel;

namespace HomeControl.Actions
{
    public class ActionsService(IDatabaseConnectionService db) : IActionsService
    {
        public async Task ExecuteActionSequenceAsync<T>(List<T> actions, IServiceProvider serviceProvider) where T : Models.DatabaseModels.Action
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

                        await eventService.ScheduleEventAsync(db, ActionType.ExecuteFeature, new ExecuteDeviceFeatureActionData
                        {
                            DeviceId = scheduleFeatureExecutionDeviceOptionActionData.DeviceId,
                            FeatureName = scheduleFeatureExecutionDeviceOptionActionData.FeatureName,
                        }, DateTime.Now.AddMinutes(scheduleFeatureExecutionDeviceOptionActionData.ExecuteIn));
                        break;
                    case ActionType.ClearIntegrationDevicesCache:
                        foreach (var cache in deviceService.IntegrationDeviceCaches)
                        {
                            cache.InvalidateAll();
                        }
                        break;
                    default: throw new NotImplementedException();
                }
            }
        }
    }
}