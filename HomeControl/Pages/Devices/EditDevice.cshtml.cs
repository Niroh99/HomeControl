using HomeControl.Attributes;
using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Integrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(IndexModel), "Edit Device", "/Devices/EditDevice")]
    public class EditDeviceModel(IDatabaseConnection db, IDeviceService deviceService) : PageModel
    {
        [FromRoute]
        public int DeviceId { get; set; }

        public Device Device { get; set; }

        public IIntegrationDevice IntegrationDevice { get; set; }

        public async Task OnGet()
        {
            Device = await db.SelectSingleAsync<Device>(DeviceId);

            if (Device == null) return;

            IntegrationDevice = deviceService.CreateIntegrationDevice(Device);
            await IntegrationDevice.InitializeAsync();
        }
    }
}