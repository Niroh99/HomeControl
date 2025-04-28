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

            public string CardHeader { get => Get<string>(); set => Set(value); }

            public string CardInfoText { get => Get<string>(); set => Set(value); }

            public string CardErrorText { get => Get<string>(); set => Set(value); }
        }

        protected override PageViewModel CreateViewModel()
        {
            return new TestViewModel(this) { Test = "ERROR!", CardHeader = "Header", CardInfoText = "Very Importand Information", CardErrorText = "Error!" };
        }

        public async Task OnGet()
        {
            var data = new TimeOfDayRoutineTriggerData
            {
                TimeOfDay = TimeOnly.Parse("20:31"),
                ActiveWeekDays = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Thursday }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(data);

            System.Diagnostics.Debug.WriteLine(json);

            var test = System.Text.Json.JsonSerializer.Deserialize<TimeOfDayRoutineTriggerData>("{\"timeOfDay\":\"20:48\",\"activeWeekDays\":\"[1,2,4,5,6,0]\",\"typeName\":\"HomeControl.DatabaseModels.TimeOfDayRoutineTriggerData\",\"display\":null,\"additionalInfo\":null}");
        }

        private async Task InsertTestRoutine()
        {
            var sunsetRoutine = new Routine
            {
                Name = "Sunrise",
                IsActive = true,
            };

            await db.InsertAsync(sunsetRoutine);

            var sunsetRoutineTrigger = new RoutineTrigger
            {
                RoutineId = sunsetRoutine.Id,
                Type = RoutineTriggerType.Sunset,
                Data = new SunsetRoutineTriggerData
                {
                    ActiveWeekDays = [.. Enum.GetValues<DayOfWeek>()]
                }
            };

            await db.InsertAsync(sunsetRoutineTrigger);

            var sunsetRoutineAction = new RoutineAction
            {
                Index = 1,
                RoutineId = sunsetRoutine.Id,
                Type = ActionType.ExecuteFeature,
                Data = new ExecuteDeviceFeatureActionData
                {
                    DeviceId = 5,
                    FeatureName = "Turn On",
                }
            };

            await db.InsertAsync(sunsetRoutineAction);

            var midnightRoutine = new Routine
            {
                Name = "Midnight",
                IsActive = true,
            };

            await db.InsertAsync(midnightRoutine);

            var midnightRoutineTrigger = new RoutineTrigger
            {
                RoutineId = midnightRoutine.Id,
                Type = RoutineTriggerType.TimeOfDay,
                Data = new TimeOfDayRoutineTriggerData
                {
                    TimeOfDay = TimeOnly.MinValue,
                    ActiveWeekDays = [.. Enum.GetValues<DayOfWeek>()]
                }
            };

            await db.InsertAsync(midnightRoutineTrigger);

            var midnightRoutineAction = new RoutineAction
            {
                Index = 1,
                RoutineId = midnightRoutine.Id,
                Type = ActionType.ExecuteFeature,
                Data = new ExecuteDeviceFeatureActionData
                {
                    DeviceId = 5,
                    FeatureName = "Turn Off",
                }
            };

            await db.InsertAsync(midnightRoutineAction);
        }

        public async Task<IActionResult> OnPostTestAjaxPost(string id)
        {
            await InsertTestRoutine();

            return await ViewModelResponse();
        }
    }
}