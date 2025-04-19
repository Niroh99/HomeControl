using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Events;
using HomeControl.Events.EventDatas;
using HomeControl.Helpers;
using HomeControl.Modeling;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages
{
    public class TestModel(IEventService eventService, IDatabaseConnection db) : ViewModelPageModel<TestModel.TestViewModel>
    {
        public class TestViewModel(ViewModelPageModelBase page) : PageViewModel(page)
        {
            public string Test { get => Get<string>(); set => Set(value); }
        }

        private IEventService _eventService = eventService;

        protected override PageViewModel CreateViewModel()
        {
            return new TestViewModel(this);
        }

        public void OnGet()
        {
            ViewModel = new TestViewModel(this) { Test = "test" };

            EnumHelper.GetValueDescription(DatabaseModels.DeviceOptionActionType.ExecuteFeature);
        }

        public async Task<IActionResult> OnPostTestAjaxPost(string id)
        {
            var test = await db.SelectAsync(WhereBuilder.Where<Device>().Compare(device => device.Id, ComparisonOperator.Equals, 2).Or().Compare(device => device.Port, ComparisonOperator.GreaterThan, 9997));

            return ViewModelResponse();
        }
    }
}