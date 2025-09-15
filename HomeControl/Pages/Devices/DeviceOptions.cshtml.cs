using HomeControl.Attributes;
using HomeControl.Models.DatabaseModels;
using HomeControl.Models.ServicesInterfaces;
using HomeControl.ViewModels.Devices;
using Microsoft.AspNetCore.Mvc;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(EditDeviceModel), "Device Options", "/Devices/DeviceOptions")]
    public class DeviceOptionsModel(IServiceProvider serviceProvider, IDatabaseConnectionService db) : ViewModelPageModel<DeviceOptionsViewModel>(serviceProvider)
    {
        [FromRoute]
        public int DeviceId { get; set; }

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

        protected override Task InitializingViewModelAsync()
        {
            ViewModel.DeviceId = DeviceId;
            return base.InitializingViewModelAsync();
        }
    }
}