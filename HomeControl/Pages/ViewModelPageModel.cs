using HomeControl.Modeling;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages
{
    public class ViewModelPageModel<T> : ViewModelPageModelBase where T : PageViewModel
    {
        public T ViewModel { get => (T)ViewModelBase; set => ViewModelBase = value; }
    }
}