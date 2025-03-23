using HomeControl.Integrations;
using HomeControl.Database;
using Microsoft.AspNetCore.Mvc;
using HomeControl.DatabaseModels;
using HomeControl.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(ManageDevicesModel), "Remove Device", "/Devices/ManageDevices/RemoveDevice")]
    public class RemoveDeviceModel(IDatabaseConnection db, IDeviceService deviceService) : PageModel
    {
        public List<IIntegrationDevice> Devices { get; } = new List<IIntegrationDevice>();

        public async Task OnGet()
        {
            await PopulateDevicesAsync(await db.SelectAllAsync<Device>());
        }

        public async Task<IActionResult> OnPostRemoveDevice(int deviceId)
        {
            var device = await db.SelectAsync<Device>(deviceId);

            await db.DeleteAsync(device);

            return Redirect("/Devices");
        }

        private async Task PopulateDevicesAsync(List<Device> deviceList)
        {
            for (int i = 0; i < deviceList.Count; i++)
            {
                var device = deviceList[i];

                var integrationDevice = deviceService.CreateIntegrationDevice(device);

                Devices.Add(integrationDevice);

                await integrationDevice.InitializeAsync();
            }
        }
    }
}