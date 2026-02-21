
namespace HomeControl.Integrations.TPLink
{
    public class DeviceCache : DefaultIntegrationDeviceCache
    {
        public override bool CanHandleDevice(Models.DatabaseModels.Device device)
        {
            return device.Type == Models.DatabaseModels.DeviceType.TPLinkSmartPlug;
        }
    }
}