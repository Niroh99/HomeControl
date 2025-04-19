using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Events;
using HomeControl.Events.EventDatas;
using System.Reflection;

namespace HomeControl.Integrations
{
    public interface IDeviceService
    {
        public static Dictionary<DeviceOptionActionType, Type> DeviceOptionActionTypeDataMap { get; } = new Dictionary<DeviceOptionActionType, Type>
        {
            { DeviceOptionActionType.ExecuteFeature, typeof(ExecuteFeatureDeviceOptionActionData) },
            { DeviceOptionActionType.ScheduleFeatureExecution, typeof(ScheduleFeatureExecutionDeviceOptionActionData) }
        };

        IIntegrationDevice CreateIntegrationDevice(Device device);

        Task<IIntegrationDevice> CreateAndInitializeIntegrationDeviceAsync(Device device);

        Task ExecuteFeatureAsync(int deviceId, string featureName);

        Task ExecuteFeatureAsync(Device device, string featureName);

        Task ExecuteDeviceOptionAsync(int deviceOptionId);
    }

    public class DeviceService(IDatabaseConnection db, IEventService eventService) : IDeviceService
    {
        static DeviceService()
        {
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var cacheType in assembly.DefinedTypes.Where(x => x.IsAssignableTo(typeof(IIntegrationDeviceCache))))
            {
                if (cacheType == typeof(DefaultIntegrationDeviceCache) || cacheType == typeof(IIntegrationDeviceCache)) continue;

                _integrationDeviceCaches.Add((IIntegrationDeviceCache)Activator.CreateInstance(cacheType));
            }

            _integrationDeviceCaches.Add(new DefaultIntegrationDeviceCache());
        }

        private static readonly List<IIntegrationDeviceCache> _integrationDeviceCaches = [];

        public static bool TryGetDeviceCache(Device device, out IIntegrationDeviceCache cache)
        {
            cache = _integrationDeviceCaches.FirstOrDefault(cache => cache.CanHandleDevice(device));

            return cache != null;
        }

        public IIntegrationDevice CreateIntegrationDevice(Device device)
        {
            var hasCache = TryGetDeviceCache(device, out var cache);

            if (hasCache && cache.TryGetIntegrationDevice(device, out var integrationDevice)) return integrationDevice;
            else
            {
                switch (device.Type)
                {
                    case DeviceType.TPLinkSmartPlug: integrationDevice = new TPLink.SmartPlug(device, device.Hostname, device.Port); break;
                    default: throw new NotImplementedException();
                }

                if (hasCache) cache.CacheDevice(device, integrationDevice);
            }

            return integrationDevice;
        }

        public async Task<IIntegrationDevice> CreateAndInitializeIntegrationDeviceAsync(Device device)
        {
            var integrationDevice = CreateIntegrationDevice(device);

            await integrationDevice.InitializeAsync();

            return integrationDevice;
        }

        public async Task ExecuteFeatureAsync(int deviceId, string featureName)
        {
            var device = await db.SelectSingleAsync<Device>(deviceId);

            await ExecuteFeatureAsync(device, featureName);
        }

        public async Task ExecuteFeatureAsync(Device device, string featureName)
        {
            ArgumentNullException.ThrowIfNull(device, nameof(device));
            ArgumentException.ThrowIfNullOrWhiteSpace(featureName, nameof(featureName));

            var integrationDevice = await CreateAndInitializeIntegrationDeviceAsync(device);

            await integrationDevice.ExecuteFeatureAsync(featureName);

            if (TryGetDeviceCache(device, out var cache)) cache.InvalidateCache(device);
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
                        var executeFeatureDeviceOptionActionData = (ExecuteFeatureDeviceOptionActionData)action.Data;

                        await ExecuteFeatureAsync(device, executeFeatureDeviceOptionActionData.FeatureName);
                        break;
                    case DeviceOptionActionType.ScheduleFeatureExecution:
                        var scheduleFeatureExecutionDeviceOptionActionData = (ScheduleFeatureExecutionDeviceOptionActionData)action.Data;

                        await eventService.ScheduleEventAsync(db, EventType.ExecuteDeviceFeature, new ExecuteDeviceFeatureEventData
                        {
                            DeviceId = deviceOption.DeviceId,
                            FeatureName = scheduleFeatureExecutionDeviceOptionActionData.FeatureName,
                        }, DateTime.Now.AddSeconds(scheduleFeatureExecutionDeviceOptionActionData.ExecuteIn));
                        break;
                    default: throw new NotImplementedException();
                }
            }
        }
    }
}