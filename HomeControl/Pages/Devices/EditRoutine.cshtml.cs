using HomeControl.Attributes;
using HomeControl.Models.DatabaseModels;
using HomeControl.Models.ServicesInterfaces;
using HomeControl.Routines;
using HomeControl.ViewModels;
using HomeControl.ViewModels.Devices;
using Microsoft.AspNetCore.Mvc;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(RoutinesModel), "Edit Routine", null)]
    public partial class EditRoutineModel(IServiceProvider serviceProvider, IDatabaseConnectionService db) : ViewModelPageModel<EditRoutineViewModel>(serviceProvider)
    {
        [FromRoute]
        public int RoutineId { get; set; }

        public string TestString()
        {
            return "TestStringValue";
        }

        public void OnGet()
        {
            
        }

        public async Task<IActionResult> OnPostToggleRoutineIsActive()
        {
            if (ViewModel.Routine == null) return null;

            ViewModel.Routine.IsActive = !ViewModel.Routine.IsActive;

            await db.Update(ViewModel.Routine).ExecuteAsync();

            return await ViewModelResponse();
        }

        public async Task<IActionResult> OnPostRename(string routineName)
        {
            if (ViewModel.Routine == null || string.IsNullOrWhiteSpace(routineName)) return null;

            ViewModel.Routine.Name = routineName;

            await db.Update(ViewModel.Routine).ExecuteAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveRoutine()
        {
            if (ViewModel.Routine == null) return null;

            await db.Delete(ViewModel.Routine).ExecuteAsync();

            return RedirectToPage("/Devices/Routines");
        }

        public async Task<IActionResult> OnPostCreateRoutineTrigger(RoutineTriggerType routineTriggerType, string newRoutineTriggerData)
        {
            var triggerDataObject = (RoutineTriggerData)System.Text.Json.JsonSerializer.Deserialize(newRoutineTriggerData, IRoutinesService.RoutineTriggerTypeDataMap[routineTriggerType], new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

            var routineTrigger = new RoutineTrigger
            {
                RoutineId = ViewModel.Routine.Id,
                Type = routineTriggerType,
                Data = triggerDataObject
            };

            await db.Insert(routineTrigger).ExecuteAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveRoutineTrigger(int triggerIdToRemove)
        {
            await db.Delete(await db.SelectSingle<RoutineTrigger>(triggerIdToRemove).ExecuteAsync()).ExecuteAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateRoutineAction(ActionType actionType, string newRoutineActionData)
        {
            var actionDataObject = (ActionData)System.Text.Json.JsonSerializer.Deserialize(newRoutineActionData, IRoutinesService.RoutineActionTypeDataMap[actionType], new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

            var routineAction = new RoutineAction
            {
                RoutineId = ViewModel.Routine.Id,
                Index = ViewModel.RoutineActions.Count + 1,
                Type = actionType,
                Data = actionDataObject
            };

            await db.Insert(routineAction).ExecuteAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveRoutineAction(int actionIdToRemove)
        {
            await db.Delete(await db.SelectSingle<RoutineAction>(actionIdToRemove).ExecuteAsync()).ExecuteAsync();

            return RedirectToPage();
        }

        protected override Task InitializingViewModelAsync()
        {
            ViewModel.RoutineId = RoutineId;
            return base.InitializingViewModelAsync();
        }
    }
}