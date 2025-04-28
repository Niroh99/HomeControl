using HomeControl.Helpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace HomeControl.TagHelpers
{
    [HtmlTargetElement("summary")]
    public class SummaryTagHelper(IFileVersionProvider fileVersionProvider, IUrlHelperFactory urlHelperFactory) : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; } = default!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.AddClass("grid-1fr-auto", HtmlEncoder.Default);

            var markerDiv = new TagBuilder("div");
            markerDiv.AddCssClass("centered-content");

            var marker = TagHelperHelper.RenderImage("~/svg/arrow-down.svg", fileVersionProvider, urlHelperFactory, ViewContext);
            marker.AddClass("summary-marker", HtmlEncoder.Default);

            markerDiv.InnerHtml.AppendHtml(marker);

            var openMarker = TagHelperHelper.RenderImage("~/svg/arrow-up.svg", fileVersionProvider, urlHelperFactory, ViewContext);
            openMarker.AddClass("summary-marker-open", HtmlEncoder.Default);

            markerDiv.InnerHtml.AppendHtml(openMarker);

            output.PostContent.AppendHtml(markerDiv);
        }
    }
}