using HomeControl.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages.Devices
{
    public class ManageDevicesModel : MenuPageModel
    {
        public ManageDevicesModel() : base(IndexModel.MenuItem)
        {

        }

        public IActionResult OnPostDiscoverDevices()
        {
            foreach (var device in Device.SelectAll())
            {
                device.Delete();
            }

            var devices = Integrations.TPLink.Discovery.Discover();

            foreach (var device in devices)
            {
                Device.Insert(new Device()
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