using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Events.EventDatas;
using HomeControl.Integrations;

namespace HomeControl.Events.EventHandlers
{
    public class ExecuteDeviceFeatureEventHandler : EventHandler<ExecuteDeviceFeatureEventData>
    {
        public override async void HandleEvent(IServiceProvider serviceProvider, ExecuteDeviceFeatureEventData data)
        {
            ArgumentNullException.ThrowIfNull(data, nameof(data));

            var deviceService = serviceProvider.GetService<IDeviceService>();

            await deviceService.ExecuteFeatureAsync(data.DeviceId, data.FeatureName);
        }
    }
}