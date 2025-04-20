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
            await CreateAndInitializeViewModel();
            await base.OnPageHandlerExecutionAsync(context, next);
        }

        public async Task<IActionResult> ViewModelResponse()
        {
            await CreateAndInitializeViewModel();
            return new JsonResult(ViewModelBase);
        }

        protected abstract PageViewModel CreateViewModel();

        private async Task CreateAndInitializeViewModel()
        {
            ViewModelBase = CreateViewModel();
            await ViewModelBase.Initialize();
        }
    }
}