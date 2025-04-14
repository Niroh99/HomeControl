using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Events;
using HomeControl.Events.EventDatas;

namespace HomeControl.Integrations
{
    public interface IDeviceService
    {
        IIntegrationDevice CreateIntegrationDevice(Device device);

        Task ExecuteFeatureAsync(int deviceId, string featureName);

        Task ExecuteFeatureAsync(Device device, string featureName);

        Task ExecuteDeviceOptionAsync(int deviceOptionId);
    }

    public class DeviceService(IDatabaseConnection db, IEventService eventService) : IDeviceService
    {
        public IIntegrationDevice CreateIntegrationDevice(Device device)
        {
            switch (device.Type)
            {
                case DeviceType.TPLinkSmartPlug: return new TPLink.SmartPlug(device, device.Hostname, device.Port);
                default: throw new NotImplementedException();
            }
        }

        public async Task ExecuteFeatureAsync(int deviceId, string featureName)
        {
            var device = await db.SelectSingleAsync<Device>(deviceId);

            await ExecuteFeatureAsync(device, featureName);
        }

        public async Task ExecuteFeatureAsync(Device device, string featureName)
        {
            ArgumentNullException.ThrowIfNull(device, nameof(device));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(featureName, nameof(featureName));

            var integrationDevice = CreateIntegrationDevice(device);

            await integrationDevice.ExecuteFeatureAsync(featureName);
        }

        public async Task ExecuteDeviceOptionAsync(int deviceOptionId)
        {
            var deviceOption = await db.SelectSingleAsync<DeviceOption>(deviceOptionId);

            ArgumentNullException.ThrowIfNull(deviceOption, nameof(deviceOptionId));

            var device = await db.SelectSingleAsync<Device>(deviceOption.DeviceId);

            var actions = await db.SelectAllAsync<DeviceOptionAction>();

            foreach (var action in actions.Where(action => action.DeviceOptionId == deviceOption.Id).OrderBy(action => action.Index))
            {
                switch (action.Type)
                {
                    case DeviceOptionActionType.ExecuteFeature:
                        var executeFeatureDeviceOptionActionData = System.Text.Json.JsonSerializer.Deserialize<ExecuteFeatureDeviceOptionActionData>(action.Data);

                        await ExecuteFeatureAsync(device, executeFeatureDeviceOptionActionData.FeatureName);
                        break;
                    case DeviceOptionActionType.ScheduleFeatureExecution:
                        var scheduleFeatureExecutionDeviceOptionActionData = System.Text.Json.JsonSerializer.Deserialize<ScheduleFeatureExecutionDeviceOptionActionData>(action.Data);

                        await eventService.ScheduleEventAsync(db, EventType.ExecuteDeviceFeature, new ExecuteDeviceFeatureEventData
                        {
                            DeviceId = deviceOption.DeviceId,
                            FeatureName = scheduleFeatureExecutionDeviceOptionActionData.FeatureName,
                        }, DateTime.Now.Add(scheduleFeatureExecutionDeviceOptionActionData.ExecuteIn));
                        break;
                    default: throw new NotImplementedException();
                }
            }
        }
    }
}