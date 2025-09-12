using HomeControl.Database;
using HomeControl.Models.DatabaseModels;
using HomeControl.Models.Integrations;
using HomeControl.Models.ServicesInterfaces;
using NTIH.Database;

namespace HomeControl.ViewModels
{
    public class DeviceOptionsViewModel(IDatabaseConnectionService db, IDeviceService deviceService) : PageViewModel
    {
        public int DeviceId { get => Get<int>(); set => Set(value); }

        public Device Device { get => Get<Device>(); set => Set(value); }

        public IIntegrationDevice IntegrationDevice { get => Get<IIntegrationDevice>(); set => Set(value); }

        public List<DeviceOption> DeviceOptions { get => Get<List<DeviceOption>>(); set => Set(value); }

        public async override Task Initialize()
        {
            Device = await db.SelectSingle<Device>(DeviceId).ExecuteAsync();

            if (Device == null) return;

            var deviceOptionsSelect = db.Select<DeviceOption>();
            deviceOptionsSelect.Where().Compare(i => i.DeviceId, ComparisonOperator.Equals, Device.Id);

            DeviceOptions = [.. await deviceOptionsSelect.ExecuteAsync()];

            IntegrationDevice = await deviceService.CreateAndInitializeIntegrationDeviceAsync(Device);
        }
    }
}