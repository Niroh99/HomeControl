using HomeControl.Integrations;
using HomeControl.Models;
using HomeControl.Sql;
using Microsoft.AspNetCore.Mvc;

namespace HomeControl.Pages.Devices
{
    public class RemoveDeviceModel : MenuPageModel
    {
        public RemoveDeviceModel() : base(IndexModel.MenuItem)
        {
        }

        public List<IDevice> Devices { get; } = new List<IDevice>();

        public override IActionResult OnGet()
        {
            base.OnGet();

            Database.Connect(() =>
            {
                Devices.AddRange(CreateDevices(Device.SelectAll()).OrderBy(x => x.DisplayName));
            });

            return null;
        }

        public IActionResult OnPostRemoveDevice(int deviceId)
        {
            return Database.Connect(() =>
            {
                var device = Device.Select(deviceId);

                device.Delete();

                return Redirect("/Devices");
            });
        }

        private IEnumerable<IDevice> CreateDevices(List<Device> deviceList)
        {
            foreach (var device in deviceList)
            {
                yield return device.Create();
            }
        }
    }
}