using HomeControl.Attributes;
using HomeControl.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.ObjectModel;
using System.Reflection;

namespace HomeControl.Helpers
{
    public static class MainMenuHelper
    {
        static MainMenuHelper()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var typeMenuPageAttributes = new Dictionary<Type, MenuItem>();

            foreach (var pageModelType in assembly.DefinedTypes.Where(x => x.IsAssignableTo(typeof(PageModel))))
            {
                TryAddMenuItem(pageModelType, out _);
            }
        }

        private static readonly Dictionary<Type, MenuItem> _pageModelTypeMenutItems = [];

        private static readonly List<MenuItem> _menuItems = [];

        public static readonly ReadOnlyCollection<MenuItem> MenuItems = _menuItems.AsReadOnly();

        public static bool TryGetMenuItem<T>(out MenuItem menuItem) where T : PageModel
        {
            return TryGetMenuItem(typeof(T), out menuItem);
        }

        public static bool TryGetMenuItem(Type pageModelType, out MenuItem menuItem)
        {
            return _pageModelTypeMenutItems.TryGetValue(pageModelType, out menuItem);
        }

        public static bool TryGetRootMenuItem(Type pageModelType, out MenuItem rootMenuItem)
        {
            if (TryGetMenuItem(pageModelType, out var menuItem))
            {
                rootMenuItem = GetRoot(menuItem);
                return true;
            }

            rootMenuItem = null;
            return false;
        }

        private static bool TryAddMenuItem(Type pageModelType, out MenuItem menuItem)
        {
            menuItem = null;

            if (_pageModelTypeMenutItems.ContainsKey(pageModelType)) return false;

            var menuPageAttribute = pageModelType.GetCustomAttribute(typeof(MenuPageAttribute)) as MenuPageAttribute;

            if (menuPageAttribute == null) return false;

            _pageModelTypeMenutItems[pageModelType] = menuItem = new MenuItem(menuPageAttribute.MenuItem, menuPageAttribute.Url);

            if (menuPageAttribute.ParentPageType == null)
            {
                _menuItems.Add(menuItem);
                return true;
            }

            if (_pageModelTypeMenutItems.TryGetValue(menuPageAttribute.ParentPageType, out var parentMenuItem)) parentMenuItem.Add(menuItem);
            else if (TryAddMenuItem(menuPageAttribute.ParentPageType, out parentMenuItem)) parentMenuItem.Add(menuItem);

            return true;
        }

        private static MenuItem GetRoot(MenuItem menuItem)
        {
            if (menuItem.Parent == null) return menuItem;

            return GetRoot(menuItem.Parent);
        }

        private static void AddMenuPageModelType(Type menuPageModelType, Dictionary<Type, (MenuItem, MenuPageAttribute)> typeMenuItems)
        {
            if (typeMenuItems.ContainsKey(menuPageModelType)) return;

            var menuPageAttribute = menuPageModelType.GetCustomAttribute(typeof(MenuPageAttribute)) as MenuPageAttribute;

            if (menuPageAttribute == null) return;

            typeMenuItems[menuPageModelType] = (new MenuItem(menuPageAttribute.MenuItem, menuPageAttribute.Url), menuPageAttribute);

            if (menuPageAttribute.ParentPageType != null) AddMenuPageModelType(menuPageAttribute.ParentPageType, typeMenuItems);
        }
    }
}