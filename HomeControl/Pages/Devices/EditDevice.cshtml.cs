using HomeControl.Attributes;
using HomeControl.Models.ServicesInterfaces;
using HomeControl.ViewModels.Devices;
using Microsoft.AspNetCore.Mvc;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(IndexModel), "Edit Device", null)]
    public partial class EditDeviceModel(IServiceProvider serviceProvider, IDatabaseConnectionService db) : ViewModelPageModel<EditDeviceViewModel>(serviceProvider)
    {
        [FromRoute]
        public int DeviceId { get; set; }

        public void OnGet()
        {
            
        }

        public async Task<IActionResult> OnPostRename(string displayName)
        {
            if (ViewModel.IntegrationDevice == null) return null;

            await ViewModel.IntegrationDevice.RenameAsync(displayName);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveDevice()
        {
            if (ViewModel.Device == null) return null;

            await db.Delete(ViewModel.Device).ExecuteAsync();

            return RedirectToPage("/Devices/Index");
        }

        protected override Task InitializingViewModelAsync()
        {
            ViewModel.DeviceId = DeviceId;
            return base.InitializingViewModelAsync();
        }
    }
}