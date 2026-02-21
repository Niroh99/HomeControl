using HomeControl.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NTIH.ViewModeling;
using System.Text.Json;

namespace HomeControl.Pages
{
    public abstract class ViewModelPageModel<T>(IServiceProvider serviceProvider) : PageModel where T : ViewModel
    {
        public static readonly JsonSerializerOptions WebSerializationOptions = new(JsonSerializerDefaults.Web);

        public T ViewModel { get; private set; }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            await CreateAndInitializeViewModel();
            await base.OnPageHandlerExecutionAsync(context, next);
        }

        public override void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            context.HttpContext.Response.Cookies.Append("PageInfo", JsonSerializer.Serialize(new PageInfo(Url.PageLink()), WebSerializationOptions));

            base.OnPageHandlerExecuted(context);
        }

        public IActionResult OnGetViewModel()
        {
            return new JsonResult(ViewModel);
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
            await InitializingViewModelAsync();
            await ViewModel.Initialize();
        }
    }
}