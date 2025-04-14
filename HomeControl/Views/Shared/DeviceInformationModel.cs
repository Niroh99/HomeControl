using HomeControl.Integrations;

namespace HomeControl.Views.Shared
{
    public class DeviceInformationModel(IIntegrationDevice integrationDevice)
    {
        public IIntegrationDevice IntegrationDevice { get; } = integrationDevice;
    }
}