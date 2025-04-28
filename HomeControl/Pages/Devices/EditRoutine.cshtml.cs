using HomeControl.Attributes;
using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Helpers;
using HomeControl.Integrations;
using HomeControl.Modeling;
using HomeControl.Routines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(RoutinesModel), "Edit Routine", null)]
    public class EditRoutineModel(IDatabaseConnection db, IDeviceService deviceService) : ViewModelPageModel<EditRoutineModel.EditRoutineViewModel>
    {
        public class EditRoutineViewModel(EditRoutineModel page, IDatabaseConnection db, IDeviceService deviceService) : PageViewModel(page)
        {
            public Routine Routine { get; set; }

            public List<RoutineTrigger> RoutineTriggers { get; } = [];

            public List<SelectListItem> TriggerTypes { get; } = [];

            public List<RoutineAction> RoutineActions { get; } = [];

            public List<SelectListItem> ActionTypes { get; } = [];

            public List<DeviceInfo> Devices { get; } = [];

            public async override Task Initialize()
            {
                Routine = await db.SelectSingleAsync<Routine>(page.RoutineId);

                if (Routine == null) return;

                RoutineTriggers.AddRange(await db.SelectAsync(WhereBuilder.Where<RoutineTrigger>()
                    .Compare(i => i.RoutineId, ComparisonOperator.Equals, Routine.Id)));

                foreach (var triggerType in IRoutinesService.RoutineTriggerTypeDataMap.Keys)
                {
                    TriggerTypes.Add(new SelectListItem(EnumHelper.GetValueDescription(triggerType), triggerType.ToString()));
                }

                RoutineActions.AddRange((await db.SelectAsync(WhereBuilder.Where<RoutineAction>()
                    .Compare(i => i.RoutineId, ComparisonOperator.Equals, Routine.Id)))
                    .OrderBy(action => action.Index));

                foreach (var actionType in IRoutinesService.RoutineActionTypeDataMap.Keys)
                {
                    ActionTypes.Add(new SelectListItem(EnumHelper.GetValueDescription(actionType), actionType.ToString()));
                }

                var devices = await db.SelectAllAsync<Device>();

                foreach (var device in devices)
                {
                    Devices.Add(await DeviceInfo.CreateAsync(device, deviceService, db));
                }
            }
        }

        [FromRoute]
        public int RoutineId { get; set; }

        public string TestString()
        {
            return "TestStringValue";
        }

        protected override PageViewModel CreateViewModel()
        {
            return new EditRoutineViewModel(this, db, deviceService);
        }

        public void OnGet()
        {
            
        }

        public async Task<IActionResult> OnPostToggleRoutineIsActive()
        {
            if (ViewModel.Routine == null) return null;

            ViewModel.Routine.IsActive = !ViewModel.Routine.IsActive;

            await db.UpdateAsync(ViewModel.Routine);

            return await ViewModelResponse();
        }

        public async Task<IActionResult> OnPostRename(string routineName)
        {
            if (ViewModel.Routine == null || string.IsNullOrWhiteSpace(routineName)) return null;

            ViewModel.Routine.Name = routineName;

            await db.UpdateAsync(ViewModel.Routine);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveRoutine()
        {
            if (ViewModel.Routine == null) return null;

            await db.DeleteAsync(ViewModel.Routine);

            return RedirectToPage("/Devices/Routines");
        }

        public async Task<IActionResult> OnPostCreateRoutineTrigger(RoutineTriggerType routineTriggerType, string newRoutineTriggerData)
        {
            var triggerDataObject = (Model)System.Text.Json.JsonSerializer.Deserialize(newRoutineTriggerData, IRoutinesService.RoutineTriggerTypeDataMap[routineTriggerType], new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

            var routineTrigger = new RoutineTrigger
            {
                RoutineId = ViewModel.Routine.Id,
                Type = routineTriggerType,
                Data = triggerDataObject
            };

            await db.InsertAsync(routineTrigger);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveRoutineTrigger(int triggerIdToRemove)
        {
            await db.DeleteAsync(await db.SelectSingleAsync<RoutineTrigger>(triggerIdToRemove));

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateRoutineAction(ActionType actionType, string newRoutineActionData)
        {
            var actionDataObject = (Model)System.Text.Json.JsonSerializer.Deserialize(newRoutineActionData, IDeviceService.DeviceOptionActionTypeDataMap[actionType], new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

            var routineAction = new RoutineAction
            {
                RoutineId = ViewModel.Routine.Id,
                Index = ViewModel.RoutineActions.Count + 1,
                Type = actionType,
                Data = actionDataObject
            };

            await db.InsertAsync(routineAction);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveRoutineAction(int actionIdToRemove)
        {
            await db.DeleteAsync(await db.SelectSingleAsync<RoutineAction>(actionIdToRemove));

            return RedirectToPage();
        }
    }
}