using HomeControl.Modeling;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages
{
    public class ViewModelPageModelBase : PageModel
    {
        protected internal Model ViewModelBase { get; set; }

        public IActionResult ViewModelResponse()
        {
            return new JsonResult(ViewModelBase);
        }
    }
}