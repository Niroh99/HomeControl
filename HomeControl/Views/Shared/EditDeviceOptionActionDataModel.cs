using HomeControl.DatabaseModels;
using HomeControl.Integrations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HomeControl.Views.Shared
{
    public class EditDeviceOptionActionDataModel(Device device, IIntegrationDevice integrationDevice, DeviceOption deviceOption, string identifier)
    {
        public string Identifier { get; } = identifier;

        public Device Device { get; } = device;

        public IIntegrationDevice IntegrationDevice { get; } = integrationDevice;

        public DeviceOption DeviceOption { get; } = deviceOption;
    }
}