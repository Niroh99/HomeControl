using HomeControl.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages
{
    public abstract class ViewModelPageModel<T>(IServiceProvider serviceProvider) : PageModel where T : PageViewModel
    {
        public T ViewModel { get; private set; }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            await CreateAndInitializeViewModel();
            await base.OnPageHandlerExecutionAsync(context, next);
        }

        public override void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            context.HttpContext.Response.Cookies.Append("ViewModel", System.Text.Json.JsonSerializer.Serialize(ViewModel));

            base.OnPageHandlerExecuted(context);
        }

        public async Task<IActionResult> ViewModelResponse()
        {
            await CreateAndInitializeViewModel();
            return new JsonResult(ViewModel);
        }

        protected virtual Task InitializingViewModelAsync()
        {
            return Task.CompletedTask;
        }

        private async Task CreateAndInitializeViewModel()
        {
            ViewModel = serviceProvider.GetService<T>();
            ViewModel.CreatePageInfo(this);
            await InitializingViewModelAsync();
            await ViewModel.Initialize();
        }
    }
}