using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace HomeControl.Helpers
{
    public static class TagHelperHelper
    {
        public static TagHelperOutput RenderImage(string src, IFileVersionProvider fileVersionProvider, IUrlHelperFactory urlHelperFactory, ViewContext viewContext)
        {
            var imageTagHelper = new ImageTagHelper(fileVersionProvider, HtmlEncoder.Default, urlHelperFactory)
            {
                ViewContext = viewContext
            };

            var imageOutput = new TagHelperOutput("img", [new TagHelperAttribute("src", src)], async (useCachedResult, encoder) => await Task.FromResult(new DefaultTagHelperContent()));
            var imageContext = new TagHelperContext([], new Dictionary<object, object>(), Guid.NewGuid().ToString());

            imageTagHelper.Process(imageContext, imageOutput);

            return imageOutput;
        }
    }
}