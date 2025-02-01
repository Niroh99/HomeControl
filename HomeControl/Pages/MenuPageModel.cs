using HomeControl.Sql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages
{
    public abstract class MenuPageModel : PageModel
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

        protected void Initialize()
        {
            ViewData[ShowMenuKey] = true;
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