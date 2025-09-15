using Microsoft.AspNetCore.Mvc;
using HomeControl.Attributes;
using HomeControl.Models.ServicesInterfaces;
using HomeControl.ViewModels.Devices;

namespace HomeControl.Pages.Devices
{
    [MenuPage(null, "Devices", "/Devices/Index")]
    public partial class IndexModel(IServiceProvider serviceProvider, IDeviceService deviceService) : ViewModelPageModel<DevicesViewModel>(serviceProvider)
    {
        public void OnGet()
        {
            
        }

        public async Task<IActionResult> OnPostExecuteFeature(int id, string featureName)
        {
            await deviceService.ExecuteFeatureAsync(id, featureName);

            return await ViewModelResponse();
        }

        public async Task<IActionResult> OnPostExecuteOption(int optionId)
        {
            await deviceService.ExecuteDeviceOptionAsync(optionId);

            return await ViewModelResponse();
        }
    }
}
