using HomeControl.Integrations;
using HomeControl.Models.DatabaseModels;
using HomeControl.Models.Integrations;
using System.Collections.ObjectModel;

namespace HomeControl.Models.ServicesInterfaces
{
    public interface IDeviceService
    {
        public static ReadOnlyDictionary<ActionType, Type> DeviceOptionActionTypeDataMap { get; } =
            new ActionType[] { ActionType.ExecuteFeature, ActionType.ScheduleFeatureExecution }
            .ToDictionary(actionType => actionType, actionType => IActionsService.ActionTypeDataMap[actionType]).AsReadOnly();

        ReadOnlyCollection<IIntegrationDeviceCache> IntegrationDeviceCaches { get; }

        bool TryGetIntegrationDeviceCache<T>(out T cache) where T : IIntegrationDeviceCache;

        IIntegrationDevice CreateIntegrationDevice(Device device);

        Task<IIntegrationDevice> CreateAndInitializeIntegrationDeviceAsync(Device device);

        Task ExecuteFeatureAsync(int deviceId, string featureName);

        Task ExecuteFeatureAsync(Device device, string featureName);

        Task ExecuteDeviceOptionAsync(int deviceOptionId);
    }
}
