using HomeControl.Database;
using HomeControl.DatabaseModels;

namespace HomeControl.Integrations
{
    public class DeviceInfo
    {
        private DeviceInfo()
        {

        }

        public Device Device { get; private set; }

        public IIntegrationDevice IntegrationDevice { get; private set; }

        public List<DeviceOption> Options { get; } = [];

        public string PrimaryOption { get => IntegrationDevice.GetExecutableFeatures().FirstOrDefault()?.Name; } 

        public static async Task<DeviceInfo> CreateAsync(Device device, IDeviceService deviceService, IDatabaseConnectionService db)
        {
            var instance = new DeviceInfo
            {
                Device = device,
                IntegrationDevice = await deviceService.CreateAndInitializeIntegrationDeviceAsync(device)
            };

            var optionsSelect = db.Select<DeviceOption>();
            optionsSelect.Where().Compare(i => i.DeviceId, ComparisonOperator.Equals, device.Id);

            instance.Options.AddRange(await optionsSelect.ExecuteAsync());

            return instance;
        }
    }
}