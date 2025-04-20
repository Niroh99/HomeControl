using HomeControl.Actions;
using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Events;
using HomeControl.Events.EventDatas;
using System.Reflection;

namespace HomeControl.Integrations
{
    public interface IDeviceService
    {
        public static Dictionary<ActionType, Type> DeviceOptionActionTypeDataMap { get; } = new Dictionary<ActionType, Type>
        {
            { ActionType.ExecuteFeature, typeof(ExecuteDeviceFeatureActionData) },
            { ActionType.ScheduleFeatureExecution, typeof(ScheduleDeviceFeatureExecutionActionData) }
        };

        bool TryGetDeviceCache<T>(out T cache) where T : IIntegrationDeviceCache;

        IIntegrationDevice CreateIntegrationDevice(Device device);

        Task<IIntegrationDevice> CreateAndInitializeIntegrationDeviceAsync(Device device);

        Task ExecuteFeatureAsync(int deviceId, string featureName);

        Task ExecuteFeatureAsync(Device device, string featureName);

        Task ExecuteDeviceOptionAsync(int deviceOptionId);
    }

    public class DeviceService(IDatabaseConnection db, IActionsService actionsService, IServiceProvider serviceProvider) : IDeviceService
    {
        static DeviceService()
        {
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var cacheType in assembly.DefinedTypes.Where(x => x.IsAssignableTo(typeof(IIntegrationDeviceCache))))
            {
                if (cacheType == typeof(DefaultIntegrationDeviceCache) || cacheType == typeof(IIntegrationDeviceCache)) continue;

                _integrationDeviceCaches.Add((IIntegrationDeviceCache)Activator.CreateInstance(cacheType));
            }
        }

        private static readonly List<IIntegrationDeviceCache> _integrationDeviceCaches = [];

        public static bool TryGetDeviceCache(Device device, out IIntegrationDeviceCache cache)
        {
            cache = _integrationDeviceCaches.FirstOrDefault(cache => cache.CanHandleDevice(device));

            return cache != null;
        }

        public bool TryGetDeviceCache<T>(out T cache) where T : IIntegrationDeviceCache
        {
            cache = _integrationDeviceCaches.OfType<T>().FirstOrDefault();

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

            var actions = await db.SelectAsync(WhereBuilder.Where<DeviceOptionAction>().Compare(i => i.DeviceOptionId, ComparisonOperator.Equals, deviceOption.Id));

            await actionsService.ExecuteActionSequenceAsync(actions, serviceProvider);
        }
    }
}