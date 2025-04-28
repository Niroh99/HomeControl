using HomeControl.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace HomeControl.TagHelpers
{
    [HtmlTargetElement("custom-select")]
    public class CustomSelectTagHelper(IHtmlGenerator generator, IFileVersionProvider fileVersionProvider, IUrlHelperFactory urlHelperFactory) : SelectTagHelper(generator)
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "select";

            var wrapper = new TagBuilder("div");
            wrapper.AddCssClass("select-wrapper");

            output.PreElement.AppendHtml(wrapper.RenderStartTag());

            var icon = TagHelperHelper.RenderImage("~/svg/arrow-down.svg", fileVersionProvider, urlHelperFactory, ViewContext);
            icon.AddClass("select-marker", HtmlEncoder.Default);

            output.PostElement.AppendHtml(icon);

            output.PostElement.AppendHtml(wrapper.RenderEndTag());

            base.Process(context, output);
        }
    }
}
