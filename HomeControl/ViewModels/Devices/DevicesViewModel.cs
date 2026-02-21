using HomeControl.Integrations;
using HomeControl.Models.ServicesInterfaces;
using HomeControl.Models.DatabaseModels;
using NTIH.ViewModeling;

namespace HomeControl.ViewModels.Devices
{
    public class DevicesViewModel(IDatabaseConnectionService db, IDeviceService deviceService) : ViewModel
    {
        public List<DeviceInfo> Devices { get => GetList<DeviceInfo>(); }

        public override async Task Initialize()
        {
            var databaseDevices = await db.Select<Device>().ExecuteAsync();

            var devices = new List<DeviceInfo>();

            foreach (var databaseDevice in databaseDevices)
            {
                devices.Add(await DeviceInfo.CreateAsync(databaseDevice, deviceService, db));
            }

            Devices.AddRange(devices.OrderBy(device => device.IntegrationDevice.DisplayName));
        }
    }
}