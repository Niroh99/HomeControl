using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HomeControl.Modeling;
using HomeControl.Integrations;
using HomeControl.Attributes;

namespace HomeControl.Pages
{
    [MenuPage(null, "Home", "/Index")]
    public class IndexModel : PageModel
    {
        
    }
}