using HomeControl.Database;
using HomeControl.Models.DatabaseModels;
using HomeControl.Models.ServicesInterfaces;
using Microsoft.AspNetCore.Mvc;
using NTIH.ViewModeling;

namespace HomeControl.Pages
{
    public class TestModel(IServiceProvider serviceProvider, IDatabaseConnectionService db) : ViewModelPageModel<TestModel.TestViewModel>(serviceProvider)
    {
        public class TestViewModel : ViewModel
        {

        }

        public async Task OnGet()
        {
            var stock = await db.Select<Stock>()
                .LeftJoin(i => i.Product)
                .BeginWhere()
                .Compare(i => i.Id, NTIH.Database.ComparisonOperator.Equals, 3)
                .EndWhere().ExecuteAsync();
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