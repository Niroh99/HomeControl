using HomeControl.Attributes;
using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Integrations;
using HomeControl.Modeling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(EditDeviceModel), "Device Options", "/Devices/DeviceOptions")]
    public class DeviceOptionsModel(IDatabaseConnection db, IDeviceService deviceService) : ViewModelPageModel<DeviceOptionsModel.DeviceOptionsViewModel>
    {
        public class DeviceOptionsViewModel(DeviceOptionsModel page, IDatabaseConnection db, IDeviceService deviceService) : PageViewModel(page)
        {
            public Device Device { get; set; }

            public IIntegrationDevice IntegrationDevice { get; set; }

            public List<DeviceOption> DeviceOptions { get; } = [];

            public async override Task Initialize()
            {
                Device = await db.SelectSingleAsync<Device>(page.DeviceId);

                if (Device == null) return;

                DeviceOptions.AddRange(await db.SelectAsync(WhereBuilder.Where<DeviceOption>().Compare(i => i.DeviceId, ComparisonOperator.Equals, Device.Id)));

                IntegrationDevice = await deviceService.CreateAndInitializeIntegrationDeviceAsync(Device);
            }
        }

        [FromRoute]
        public int DeviceId { get; set; }

        protected override PageViewModel CreateViewModel()
        {
            return new DeviceOptionsViewModel(this, db, deviceService);
        }

        public void OnGet()
        {
            
        }

        public async Task<IActionResult> OnPostCreateDeviceOption(string deviceOptionName)
        {
            if (string.IsNullOrWhiteSpace(deviceOptionName)) return null;

            var newDeviceOption = new DeviceOption()
            {
                DeviceId = ViewModel.Device.Id,
                Name = deviceOptionName
            };

            await db.InsertAsync(newDeviceOption);

            return RedirectToPage("/Devices/EditDeviceOption", new { DeviceOptionId = newDeviceOption.Id });
        }
    }
}