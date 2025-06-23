using HomeControl.Attributes;
using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Helpers;
using HomeControl.Integrations;
using HomeControl.Modeling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(EditDeviceModel), "Edit Device Option", null)]
    public class EditDeviceOptionModel(IDatabaseConnectionService db, IDeviceService deviceService) : ViewModelPageModel<EditDeviceOptionModel.EditDeviceOptionViewModel>
    {
        public class EditDeviceOptionViewModel(EditDeviceOptionModel page, IDatabaseConnectionService db, IDeviceService deviceService) : PageViewModel(page)
        {
            public Device Device { get; set; }

            public IIntegrationDevice IntegrationDevice { get; set; }

            public DeviceOption DeviceOption { get; set; }

            public List<DeviceOptionAction> DeviceOptionActions { get; } = [];

            public List<SelectListItem> DeviceOptionActionTypes { get; } = [];

            public async override Task Initialize()
            {
                DeviceOption = await db.SelectSingle<DeviceOption>(page.DeviceOptionId).ExecuteAsync();

                if (DeviceOption == null) return;

                Device = await db.SelectSingle<Device>(DeviceOption.DeviceId).ExecuteAsync();
                IntegrationDevice = await deviceService.CreateAndInitializeIntegrationDeviceAsync(Device);

                var deviceOptionActionsSelect = db.Select<DeviceOptionAction>();
                deviceOptionActionsSelect.Where().Compare(i => i.DeviceOptionId, ComparisonOperator.Equals, DeviceOption.Id);

                DeviceOptionActions.AddRange((await deviceOptionActionsSelect.ExecuteAsync()).OrderBy(action => action.Index));

                DeviceOptionActionTypes.AddRange(IDeviceService.DeviceOptionActionTypeDataMap
                    .Select(type => new SelectListItem(EnumHelper.GetValueDescription(type.Key), type.Key.ToString())));
            }
        }

        [FromRoute]
        public int DeviceOptionId { get; set; }

        public string TestString()
        {
            return "TestStringValue";
        }

        protected override PageViewModel CreateViewModel()
        {
            return new EditDeviceOptionViewModel(this, db, deviceService);
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
            var actionDataObject = (Model)System.Text.Json.JsonSerializer.Deserialize(newDeviceOptionActionData, IDeviceService.DeviceOptionActionTypeDataMap[deviceOptionActionType], new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

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