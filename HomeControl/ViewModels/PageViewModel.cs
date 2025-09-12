using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NTIH.ViewModeling;

namespace HomeControl.ViewModels
{
    public abstract class PageViewModel : ViewModel
    {
        public PageInfo PageInfo { get; private set; }
        
        public void CreatePageInfo(PageModel page)
        {
            PageInfo = new PageInfo(page.Url.PageLink());
        }
    }

    public class PageInfo(string url)
    {
        public string Url { get; } = url;
    }
}