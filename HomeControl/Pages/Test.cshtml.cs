using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Events;
using HomeControl.Events.EventDatas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages
{
    public class TestModel(IEventService eventService, IDatabaseConnection db) : PageModel
    {
        private IEventService _eventService = eventService;

        public void OnGet()
        {
        }

        public async void OnPostTest()
        {
            var deviceOption = new DeviceOption
            {
                DeviceId = 6,
                Name = "20 min"
            };

            await db.InsertAsync(deviceOption);

            var deviceOptionAction = new DeviceOptionAction
            {
                DeviceOptionId = deviceOption.Id,
                Index = 1,
                Type = DeviceOptionActionType.ExecuteFeature,
                Data = System.Text.Json.JsonSerializer.Serialize(new ExecuteFeatureDeviceOptionActionData
                {
                    FeatureName = Integrations.TPLink.SmartPlug.TurnOnFeatureName,
                }),
            };
            
            await db.InsertAsync(deviceOptionAction);

            var secondDeviceOptionAction = new DeviceOptionAction
            {
                DeviceOptionId = deviceOption.Id,
                Index = 2,
                Type = DeviceOptionActionType.ScheduleFeatureExecution,
                Data = System.Text.Json.JsonSerializer.Serialize(new ScheduleFeatureExecutionDeviceOptionActionData
                {
                    FeatureName = Integrations.TPLink.SmartPlug.TurnOffFeatureName,
                    ExecuteIn = TimeSpan.FromMinutes(20)
                }),
            };

            await db.InsertAsync(secondDeviceOptionAction);
        }
    }
}