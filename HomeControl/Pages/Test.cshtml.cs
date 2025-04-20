using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Events;
using HomeControl.Events.EventDatas;
using HomeControl.Helpers;
using HomeControl.Modeling;
using HomeControl.Routines;
using HomeControl.Weather;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace HomeControl.Pages
{
    public class TestModel(IRoutinesService routinesService, IDatabaseConnection db) : ViewModelPageModel<TestModel.TestViewModel>
    {
        public class TestViewModel(ViewModelPageModelBase page) : PageViewModel(page)
        {
            public string Test { get => Get<string>(); set => Set(value); }
        }

        protected override PageViewModel CreateViewModel()
        {
            return new TestViewModel(this);
        }

        public async Task OnGet()
        {
            await routinesService.ExecuteActiveRoutinesAsync();
        }

        private async Task InsertTestRoutine()
        {
            var routine = new Routine
            {
                IsActive = true,
            };

            await db.InsertAsync(routine);

            var routineTrigger = new RoutineTrigger
            {
                RoutineId = routine.Id,
                Type = RoutineTriggerType.Interval,
                Data = new IntervalTriggerData
                {
                    Interval = TimeSpan.FromMinutes(2),
                }
            };

            await db.InsertAsync(routineTrigger);

            var firstRoutineAction = new RoutineAction
            {
                Index = 1,
                RoutineId = routine.Id,
                Type = ActionType.ExecuteFeature,
                Data = new ExecuteDeviceFeatureActionData
                {
                    DeviceId = 2,
                    FeatureName = "Turn On",
                }
            };

            await db.InsertAsync(firstRoutineAction);

            var secondRoutineAction = new RoutineAction
            {
                Index = 2,
                RoutineId = routine.Id,
                Type = ActionType.ScheduleFeatureExecution,
                Data = new ScheduleDeviceFeatureExecutionActionData
                {
                    DeviceId = 2,
                    ExecuteIn = 1,
                    FeatureName = "Turn Off"
                }
            };

            await db.InsertAsync(secondRoutineAction);
        }

        public async Task<IActionResult> OnPostTestAjaxPost(string id)
        {
            var test = await db.SelectAsync(WhereBuilder.Where<Device>().Compare(device => device.Id, ComparisonOperator.Equals, 2).Or().Compare(device => device.Port, ComparisonOperator.GreaterThan, 9997));

            return await ViewModelResponse();
        }
    }
}