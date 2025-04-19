using HomeControl.Modeling;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages
{
    public abstract class ViewModelPageModel<T> : ViewModelPageModelBase where T : PageViewModel
    {
        public T ViewModel { get => (T)ViewModelBase; set => ViewModelBase = value; }

        protected abstract override PageViewModel CreateViewModel();
    }
}