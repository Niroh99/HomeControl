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
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace HomeControl.Pages
{
    public class TestModel(IDatabaseConnection db) : ViewModelPageModel<TestModel.TestViewModel>
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
            return await ViewModelResponse();
        }
    }
}