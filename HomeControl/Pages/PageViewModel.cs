using HomeControl.Modeling;
using Microsoft.AspNetCore.Mvc;

namespace HomeControl.Pages
{
    public class PageViewModel(ViewModelPageModelBase page) : Model
    {
        public PageInfo PageInfo { get; } = new PageInfo(page.Url.PageLink());
    }

    public class PageInfo(string url)
    {
        public string Url { get; } = url;
    }
}