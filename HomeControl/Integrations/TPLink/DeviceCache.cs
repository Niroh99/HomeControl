
namespace HomeControl.Integrations.TPLink
{
    public class DeviceCache : DefaultIntegrationDeviceCache
    {
        public override bool CanHandleDevice(DatabaseModels.Device device)
        {
            return device.Type == DatabaseModels.DeviceType.TPLinkSmartPlug;
        }
    }
}