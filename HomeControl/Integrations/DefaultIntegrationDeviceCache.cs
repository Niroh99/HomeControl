using HomeControl.DatabaseModels;

namespace HomeControl.Integrations
{
    public abstract class DefaultIntegrationDeviceCache : IIntegrationDeviceCache
    {
        private readonly Dictionary<int, IIntegrationDevice> _devices = [];

        public abstract bool CanHandleDevice(Device device);

        public bool TryGetIntegrationDevice(Device device, out IIntegrationDevice integrationDevice)
        {
            if (_devices.TryGetValue(device.Id, out var tpLinkDevice))
            {
                integrationDevice = tpLinkDevice;
                return true;
            }

            integrationDevice = default;
            return false;
        }

        public void CacheDevice(Device device, IIntegrationDevice integrationDevice)
        {
            if (!integrationDevice.AllowCaching) return;

            _devices[device.Id] = integrationDevice;
        }

        public void InvalidateCache(Device device)
        {
            _devices.Remove(device.Id);
        }

        public void InvalidateAll()
        {
            _devices.Clear();
        }
    }
}