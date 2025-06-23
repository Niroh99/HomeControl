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
    public class DeviceOptionsModel(IDatabaseConnectionService db, IDeviceService deviceService) : ViewModelPageModel<DeviceOptionsModel.DeviceOptionsViewModel>
    {
        public class DeviceOptionsViewModel(DeviceOptionsModel page, IDatabaseConnectionService db, IDeviceService deviceService) : PageViewModel(page)
        {
            public Device Device { get; set; }

            public IIntegrationDevice IntegrationDevice { get; set; }

            public List<DeviceOption> DeviceOptions { get; } = [];

            public async override Task Initialize()
            {
                Device = await db.SelectSingle<Device>(page.DeviceId).ExecuteAsync();

                if (Device == null) return;

                var deviceOptionsSelect = db.Select<DeviceOption>();
                deviceOptionsSelect.Where().Compare(i => i.DeviceId, ComparisonOperator.Equals, Device.Id);

                DeviceOptions.AddRange(await deviceOptionsSelect.ExecuteAsync());

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

            await db.Insert(newDeviceOption).ExecuteAsync();

            return RedirectToPage("/Devices/EditDeviceOption", new { DeviceOptionId = newDeviceOption.Id });
        }
    }
}