namespace HomeControl.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MenuPageAttribute : Attribute
    {
        public MenuPageAttribute(Type parentPageType, string menuItem, string url)
        {
            ParentPageType = parentPageType;
            MenuItem = menuItem;
            Url = url;
        }

        public Type ParentPageType { get; }

        public string MenuItem { get; }

        public string Url { get; }
    }
}