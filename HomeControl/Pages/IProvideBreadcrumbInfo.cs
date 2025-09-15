using HomeControl.Attributes;

namespace HomeControl.Pages
{
    public interface IProvideBreadcrumbInfo
    {
        string GetPageTitle();

        string GetParentPageTitle(HirarchyPageAttribute hirarchyPageAttribute);
    }
}