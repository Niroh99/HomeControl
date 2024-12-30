using HomeControl.Integrations;
using HomeControl.Models;
using Microsoft.AspNetCore.Mvc;

namespace HomeControl.Pages.Devices
{
    public class RemoveDeviceModel : MenuPageModel
    {
        public RemoveDeviceModel() : base(IndexModel.MenuItem)
        {
        }

        public List<IDevice> Devices { get; } = new List<IDevice>();

        public override void OnGet()
        {
            base.OnGet();

            Devices.AddRange(CreateDevices(Device.SelectAll()).OrderBy(x => x.DisplayName));
        }

        public IActionResult OnPostRemoveDevice(int deviceId)
        {
            var device = Device.Select(deviceId);

            device.Delete();

            return Redirect("/Devices");
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