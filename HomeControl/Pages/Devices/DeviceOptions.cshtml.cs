using HomeControl.Attributes;
using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Integrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(EditDeviceModel), "Device Options", "/Devices/DeviceOptions")]
    public class DeviceOptionsModel(IDatabaseConnection db, IDeviceService deviceService) : PageModel
    {
        [FromRoute]
        public int DeviceId { get; set; }

        public Device Device { get; set; }

        public IIntegrationDevice IntegrationDevice { get; set; }

        public List<DeviceOption> DeviceOptions { get; } = [];

        public async Task OnGet()
        {
            Device = await db.SelectSingleAsync<Device>(DeviceId);

            if (Device == null) return;

            DeviceOptions.AddRange(await db.SelectAsync(WhereBuilder.Where<DeviceOption>().Compare(i => i.DeviceId, ComparisonOperator.Equals, Device.Id)));

            IntegrationDevice = deviceService.CreateIntegrationDevice(Device);
            await IntegrationDevice.InitializeAsync();
        }
    }
}