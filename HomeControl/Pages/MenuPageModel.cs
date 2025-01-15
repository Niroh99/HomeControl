using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages
{
    public class MenuPageModel : PageModel
    {
        public const string ShowMenuKey = "ShowMenu";
        public const string MenuItemKey = "MenuItem";

        public static readonly List<MenuItem> MenuItems = new()
        {
            new ("Home", "/"),
            new (Devices.IndexModel.MenuItem, Devices.IndexModel.MenuItemUrl),
            new (Media.IndexModel.MenuItem, Media.IndexModel.PageUrl)
        };

        public MenuPageModel(string menuItem)
        {
            _menuItem = menuItem;
        }

        private readonly string _menuItem;

        public virtual IActionResult OnGet()
        {
            ViewData[ShowMenuKey] = true;
            ViewData[MenuItemKey] = _menuItem;

            return null;
        }
    }

    public class MenuItem
    {
        public MenuItem(string name, string url)
        {
            Name = name;
            Url = url;
        }

        public string Name { get; }

        public string Url { get; }
    }
}