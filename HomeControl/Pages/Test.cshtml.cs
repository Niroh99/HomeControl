using HomeControl.Database;
using HomeControl.Models.DatabaseModels;
using HomeControl.Models.ServicesInterfaces;
using HomeControl.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HomeControl.Pages
{
    public class TestModel(IDatabaseConnectionService db) : ViewModelPageModel<TestModel.TestViewModel>
    {
        public class TestViewModel(ViewModelPageModelBase page) : PageViewModel(page)
        {

        }

        protected override PageViewModel CreateViewModel()
        {
            return new TestViewModel(this);
        }

        public async Task OnGet()
        {

        }

        public async Task<IActionResult> OnPostTestAjaxPost(string id)
        {
            var select = db.Select<Stock>();

            select.LeftJoin(i => i.Product);

            var stock = await select.ExecuteAsync();

            return await ViewModelResponse();
        }
    }
}