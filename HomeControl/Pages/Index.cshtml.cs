using Microsoft.AspNetCore.Mvc.RazorPages;
using HomeControl.Attributes;

namespace HomeControl.Pages
{
    [HirarchyPage(typeof(IndexModel), null, "Home", "/Index")]
    public class IndexModel : PageModel
    {
        
    }
}