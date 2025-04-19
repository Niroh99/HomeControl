using HomeControl.Modeling;
using Microsoft.AspNetCore.Mvc;

namespace HomeControl.Pages
{
    public abstract class PageViewModel(ViewModelPageModelBase page) : Model
    {
        public PageInfo PageInfo { get; } = new PageInfo(page.Url.PageLink());

        public virtual Task Initialize()
        {
            return Task.CompletedTask;
        }
    }

    public class PageInfo(string url)
    {
        public string Url { get; } = url;
    }
}