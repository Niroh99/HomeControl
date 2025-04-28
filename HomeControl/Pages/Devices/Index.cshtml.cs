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

                var devices = new List<DeviceInfo>();

                foreach (var databaseDevice in databaseDevices)
                {
                    devices.Add(await DeviceInfo.CreateAsync(databaseDevice, deviceService, db));
                }

                Devices.AddRange(devices.OrderBy(device => device.IntegrationDevice.DisplayName));
            }
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

            return await ViewModelResponse();
        }

        public async Task<IActionResult> OnPostExecuteOption(int optionId)
        {
            await deviceService.ExecuteDeviceOptionAsync(optionId);

            return await ViewModelResponse();
        }
    }
}
