using HomeControl.Models.DatabaseModels;
using HomeControl.Models.Integrations;
using HomeControl.Models.ServicesInterfaces;

namespace HomeControl.ViewModels.Devices
{
    public class EditDeviceViewModel(IDatabaseConnectionService db, IDeviceService deviceService) : PageViewModel
    {
        public int DeviceId { get => Get<int>(); set => Set(value); }

        public Device Device { get => Get<Device>(); set => Set(value); }

        public IIntegrationDevice IntegrationDevice { get => Get<IIntegrationDevice>(); set => Set(value); }

        public async override Task Initialize()
        {
            Device = await db.SelectSingle<Device>(DeviceId).ExecuteAsync();

            if (Device == null) return;

            IntegrationDevice = await deviceService.CreateAndInitializeIntegrationDeviceAsync(Device);
        }
    }
}