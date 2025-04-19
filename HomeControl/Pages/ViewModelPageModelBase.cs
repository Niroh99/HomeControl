using HomeControl.Modeling;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace HomeControl.Pages
{
    public abstract class ViewModelPageModelBase : PageModel
    {
        protected internal PageViewModel ViewModelBase { get; set; }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            ViewModelBase = CreateViewModel();
            await ViewModelBase.Initialize();
            await base.OnPageHandlerExecutionAsync(context, next);
        }

        protected abstract PageViewModel CreateViewModel();

        public IActionResult ViewModelResponse()
        {
            return new JsonResult(ViewModelBase);
        }
    }
}