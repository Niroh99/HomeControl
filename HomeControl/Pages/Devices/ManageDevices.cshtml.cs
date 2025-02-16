using HomeControl.Database;
using Microsoft.AspNetCore.Mvc;
using HomeControl.DatabaseModels;
using HomeControl.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(IndexModel), "Manage Devices", "/Devices/ManageDevices")]
    public class ManageDevicesModel(IDatabaseConnection db) : PageModel
    {
        public void OnGet()
        {
            
        }

        public async Task<IActionResult> OnPostDiscoverDevices()
        {
            foreach (var device in await db.SelectAllAsync<Device>())
            {
                await db.DeleteAsync(device);
            }

            var devices = Integrations.TPLink.Discovery.Discover();

            foreach (var device in devices)
            {
                await db.InsertAsync(new Device()
                {
                    Type = device.DeviceType,
                    Hostname = device.Hostname,
                    Port = device.Port,
                });
            }

            return Redirect("/Devices");
        }
    }
}