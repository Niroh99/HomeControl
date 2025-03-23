using HomeControl.Integrations;
using HomeControl.Database;
using Microsoft.AspNetCore.Mvc;
using HomeControl.DatabaseModels;
using HomeControl.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages.Devices
{
    [MenuPage(null, "Devices", "/Devices/Index")]
    public class IndexModel(IDatabaseConnection db, IDeviceService deviceService) : PageModel
    {
        public class DeviceInfo(IIntegrationDevice device, IEnumerable<DeviceOption> options)
        {
            public IIntegrationDevice Device { get; } = device;

            public IEnumerable<DeviceOption> Options { get; } = options;
        }

        public List<DeviceInfo> Devices { get; } = [];

        public async Task OnGet()
        {
            await PopulateDevicesAsync(await db.SelectAllAsync<Device>());
        }

        public async Task<IActionResult> OnPostExecuteFeature(int deviceId, string featureName)
        {
            await deviceService.ExecuteFeatureAsync(deviceId, featureName);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExecuteOption(int optionId)
        {
            await deviceService.ExecuteDeviceOptionAsync(optionId);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRename(int deviceId, string displayName)
        {
            var device = await db.SelectAsync<Device>(deviceId);

            var integrationDevice = deviceService.CreateIntegrationDevice(device);

            await integrationDevice.RenameAsync(displayName);

            return RedirectToPage();
        }

        private async Task PopulateDevicesAsync(List<Device> databaseDevices)
        {
            var deviceOptions = await db.SelectAllAsync<DeviceOption>();

            for (int i = 0; i < databaseDevices.Count; i++)
            {
                var databaseDevice = databaseDevices[i];

                var integrationDevice = deviceService.CreateIntegrationDevice(databaseDevice);

                Devices.Add(new DeviceInfo(integrationDevice, deviceOptions.Where(option => option.DeviceId == databaseDevice.Id)));

                await integrationDevice.InitializeAsync();
            }
        }
    }
}
