using HomeControl.Helpers;
using HomeControl.Models.Modeling;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace HomeControl.TagHelpers
{
    [HtmlTargetElement("card-element")]
    public class CardElementTagHelper(IDisplayFactory displayFactory, IFileVersionProvider fileVersionProvider, IUrlHelperFactory urlHelperFactory) : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; } = default!;

        public string IconSource { get; set; }

        public IDisplayable Displayable { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.AddClass("centered-content", HtmlEncoder.Default);

            if (Displayable != null)
            {
                var display = await displayFactory.CreateDisplayAsync(Displayable);

                output.Attributes.Add("header", display.Display);
                output.Attributes.Add("InfoText", display.AdditionalInfo);
            }

            if (!string.IsNullOrWhiteSpace(IconSource))
            {
                output.AddClass("grid-auto-1fr-auto", HtmlEncoder.Default);

                var imageContainer = new TagBuilder("div");
                imageContainer.AddCssClass("centered-content");

                imageContainer.InnerHtml.AppendHtml(TagHelperHelper.RenderImage(IconSource, fileVersionProvider, urlHelperFactory, ViewContext));

                output.PreContent.AppendHtml(imageContainer);
            }

            var headerContainer = new TagBuilder("div");
            headerContainer.AddCssClass("centered-content");

            var header = new TagBuilder("strong");
            header.Attributes.Add("name", "Header");

            headerContainer.InnerHtml.AppendHtml(header);
            headerContainer.InnerHtml.AppendHtml(new TagBuilder("br") { TagRenderMode = TagRenderMode.SelfClosing });

            var infoText = new TagBuilder("span");
            infoText.AddCssClass("info-text");
            infoText.AddCssClass("lesser");
            infoText.Attributes.Add("name", "InfoText");

            headerContainer.InnerHtml.AppendHtml(infoText);

            var errorText = new TagBuilder("b");
            errorText.AddCssClass("error-text");
            errorText.Attributes.Add("name", "ErrorText");

            headerContainer.InnerHtml.AppendHtml(errorText);

            output.PreContent.AppendHtml(headerContainer);
        }
    }
}