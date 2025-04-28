using HomeControl.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace HomeControl.TagHelpers
{
    [HtmlTargetElement("card-a")]
    public class CardAnchorTagHelper(IFileVersionProvider fileVersionProvider, IUrlHelperFactory urlHelperFactory, IHtmlGenerator htmlGenerator) : AnchorTagHelper(htmlGenerator)
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            output.TagName = "a";

            output.AddClass("card-a", HtmlEncoder.Default);

            var contentDiv = new TagBuilder("div");
            contentDiv.AddCssClass("grid-1fr-auto");

            var markerDiv = new TagBuilder("div");
            markerDiv.AddCssClass("centered-content");

            var markerImage = TagHelperHelper.RenderImage("~/svg/arrow-right.svg", fileVersionProvider, urlHelperFactory, ViewContext);

            markerDiv.InnerHtml.AppendHtml(markerImage);

            contentDiv.InnerHtml.AppendHtml(markerDiv);

            output.PreContent.AppendHtml(contentDiv.RenderStartTag());

            output.PostContent.AppendHtml(contentDiv.RenderBody());
            output.PostContent.AppendHtml(contentDiv.RenderEndTag());
        }
    }
}