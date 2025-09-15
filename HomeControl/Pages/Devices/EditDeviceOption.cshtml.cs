using HomeControl.Attributes;
using HomeControl.Models.DatabaseModels;
using HomeControl.Models.ServicesInterfaces;
using HomeControl.ViewModels.Devices;
using Microsoft.AspNetCore.Mvc;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(EditDeviceModel), "Edit Device Option", null)]
    public partial class EditDeviceOptionModel(IServiceProvider serviceProvider, IDatabaseConnectionService db, IDeviceService deviceService) : ViewModelPageModel<EditDeviceOptionViewModel>(serviceProvider)
    {
        [FromRoute]
        public int DeviceOptionId { get; set; }

        public string TestString()
        {
            return "TestStringValue";
        }

        protected override Task InitializingViewModelAsync()
        {
            ViewModel.DeviceOptionId = DeviceOptionId;
            return base.InitializingViewModelAsync();
        }

        public void OnGet()
        {

        }

        public async Task OnPostExecute()
        {
            await deviceService.ExecuteDeviceOptionAsync(ViewModel.DeviceOption.Id);
        }

        public async Task<IActionResult> OnPostRename(string deviceOptionName)
        {
            if (ViewModel.DeviceOption == null || string.IsNullOrWhiteSpace(deviceOptionName)) return null;

            ViewModel.DeviceOption.Name = deviceOptionName;

            await db.Update(ViewModel.DeviceOption).ExecuteAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveDeviceOption()
        {
            if (ViewModel.DeviceOption == null) return null;

            await db.Delete(ViewModel.DeviceOption).ExecuteAsync();

            return RedirectToPage("/Devices/DeviceOptions", new { ViewModel.DeviceOption.DeviceId });
        }

        public async Task<IActionResult> OnPostCreateDeviceOptionAction(ActionType deviceOptionActionType, string newDeviceOptionActionData)
        {
            var actionDataObject = (ActionData)System.Text.Json.JsonSerializer.Deserialize(newDeviceOptionActionData, IDeviceService.DeviceOptionActionTypeDataMap[deviceOptionActionType], new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

            var deviceOptionAction = new DeviceOptionAction
            {
                DeviceOptionId = ViewModel.DeviceOption.Id,
                Index = ViewModel.DeviceOptionActions.Count + 1,
                Type = deviceOptionActionType,
                Data = actionDataObject
            };

            await db.Insert(deviceOptionAction).ExecuteAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveDeviceOptionAction(int actionIdToRemove)
        {
            await db.Delete(await db.SelectSingle<DeviceOptionAction>(actionIdToRemove).ExecuteAsync()).ExecuteAsync();

            return RedirectToPage();
        }
    }
}