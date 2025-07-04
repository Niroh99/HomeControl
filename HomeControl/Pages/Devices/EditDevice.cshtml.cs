using HomeControl.Attributes;
using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Integrations;
using HomeControl.Modeling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(IndexModel), "Edit Device", null)]
    public class EditDeviceModel(IDatabaseConnectionService db, IDeviceService deviceService) : ViewModelPageModel<EditDeviceModel.EditDeviceViewModel>
    {
        public class EditDeviceViewModel(EditDeviceModel page, IDatabaseConnectionService db, IDeviceService deviceService) : PageViewModel(page)
        {
            public Device Device { get; set; }

            public IIntegrationDevice IntegrationDevice { get; set; }

            public async override Task Initialize()
            {
                Device = await db.SelectSingle<Device>(page.DeviceId).ExecuteAsync();

                if (Device == null) return;

                IntegrationDevice = await deviceService.CreateAndInitializeIntegrationDeviceAsync(Device);
            }
        }

        [FromRoute]
        public int DeviceId { get; set; }

        protected override PageViewModel CreateViewModel()
        {
            return new EditDeviceViewModel(this, db, deviceService);
        }

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
    }
}