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
            new (Devices.IndexModel.MenuItem, Devices.IndexModel.MenuItemUrl)
        };

        public MenuPageModel(string menuItem)
        {
            _showMenu = true;
            _menuItem = menuItem;
        }

        private bool _showMenu;
        private string _menuItem;

        public virtual void OnGet()
        {
            ViewData[ShowMenuKey] = _showMenu;
            ViewData[MenuItemKey] = _menuItem;
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