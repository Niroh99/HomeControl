using HomeControl.Integrations;
using HomeControl.Database;
using Microsoft.AspNetCore.Mvc;
using HomeControl.DatabaseModels;
using HomeControl.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(ManageDevicesModel), "Remove Device", "/Devices/ManageDevices/RemoveDevice")]
    public class RemoveDeviceModel(IDatabaseConnection db) : PageModel
    {
        public List<IDevice> Devices { get; } = new List<IDevice>();

        public async Task<IActionResult> OnGet()
        {
            await PopulateDevicesAsync(await db.SelectAllAsync<Device>());

            return null;
        }

        public IActionResult OnPostRemoveDevice(int deviceId)
        {
            var device = db.SelectAsync<Device>(deviceId).Result;

            db.DeleteAsync(device).Wait();

            return Redirect("/Devices");
        }

        private async Task PopulateDevicesAsync(List<Device> deviceList)
        {
            for (int i = 0; i < deviceList.Count; i++)
            {
                var device = deviceList[i];

                var integrationDevice = device.Create();

                Devices.Add(integrationDevice);

                await integrationDevice.InitializeAsync();
            }
        }
    }
}