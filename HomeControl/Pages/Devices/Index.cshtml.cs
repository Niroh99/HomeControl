using HomeControl.Integrations;
using HomeControl.Database;
using Microsoft.AspNetCore.Mvc;
using HomeControl.DatabaseModels;
using HomeControl.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HomeControl.Modeling;

namespace HomeControl.Pages.Devices
{
    [MenuPage(null, "Devices", "/Devices/Index")]
    public class IndexModel(IDatabaseConnection db, IDeviceService deviceService) : ViewModelPageModel<IndexModel.DevicesViewModel>
    {
        public class DevicesViewModel(IndexModel page, IDatabaseConnection db, IDeviceService deviceService) : PageViewModel(page)
        {
            public List<DeviceInfo> Devices { get; } = [];

            public override async Task Initialize()
            {
                await PopulateDevicesAsync();
            }

            private async Task PopulateDevicesAsync()
            {
                var databaseDevices = await db.SelectAllAsync<Device>();

                var deviceOptions = await db.SelectAllAsync<DeviceOption>();

                var devices = new List<DeviceInfo>();

                foreach (var databaseDevice in databaseDevices)
                {
                    var integrationDevice = await deviceService.CreateAndInitializeIntegrationDeviceAsync(databaseDevice);

                    devices.Add(new DeviceInfo(integrationDevice, deviceOptions.Where(option => option.DeviceId == databaseDevice.Id)));
                }

                Devices.AddRange(devices.OrderBy(device => device.Device.DisplayName));
            }
        }

        public class DeviceInfo(IIntegrationDevice device, IEnumerable<DeviceOption> options)
        {
            public IIntegrationDevice Device { get; } = device;

            public IEnumerable<DeviceOption> Options { get; } = options;

            public string PrimaryOption { get; } = device.GetExecutableFeatures().FirstOrDefault()?.Name;
        }

        protected override PageViewModel CreateViewModel()
        {
            return new DevicesViewModel(this, db, deviceService);
        }

        public void OnGet()
        {
            
        }

        public async Task<IActionResult> OnPostExecuteFeature(int id, string featureName)
        {
            await deviceService.ExecuteFeatureAsync(id, featureName);

            return ViewModelResponse();
        }

        public async Task<IActionResult> OnPostExecuteOption(int optionId)
        {
            await deviceService.ExecuteDeviceOptionAsync(optionId);

            return ViewModelResponse();
        }
    }
}
