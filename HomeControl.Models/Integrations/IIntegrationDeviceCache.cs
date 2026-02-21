using HomeControl.Models.DatabaseModels;
using HomeControl.Models.Integrations;

namespace HomeControl.Integrations
{
    public interface IIntegrationDeviceCache
    {
        bool CanHandleDevice(Device device);

        bool TryGetIntegrationDevice(Device device, out IIntegrationDevice integrationDevice);

        void CacheDevice(Device device, IIntegrationDevice integrationDevice);

        void InvalidateCache(Device device);

        void InvalidateAll();
    }
}