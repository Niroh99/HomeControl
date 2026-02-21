using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class HirarchyPageAttribute : Attribute
    {
        public HirarchyPageAttribute(Type pageType, Type parentPageType, string title, string url)
        {
            PageType = pageType;
            ParentPageType = parentPageType;
            Title = title;
            Url = url;
        }

        public Type PageType { get; }

        public Type ParentPageType { get; }

        public string Title { get; }

        public string Url { get; }
    }
}