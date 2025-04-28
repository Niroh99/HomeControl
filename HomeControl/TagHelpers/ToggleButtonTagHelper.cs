using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HomeControl.TagHelpers
{
    [HtmlTargetElement("toggle-button")]
    public class ToggleButtonTagHelper : TagHelper
    {
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "input";
            output.Attributes.SetAttribute(new TagHelperAttribute("type", "checkbox"));
            output.Attributes.SetAttribute(new TagHelperAttribute("hidden"));

            await RenderToggleButtonElements(output);
        }

        public static async Task RenderToggleButtonElements(TagHelperOutput output)
        {
            var containerLabel = new TagBuilder("label");
            containerLabel.AddCssClass("toggle-button-container");

            output.PreElement.AppendHtml(containerLabel.RenderStartTag());

            var displayDiv = new TagBuilder("div");
            displayDiv.AddCssClass("toggle-button");

            var childContent = await output.GetChildContentAsync();

            childContent.MoveTo(displayDiv.InnerHtml);

            output.PostElement.AppendHtml(displayDiv);

            output.PostElement.AppendHtml(containerLabel.RenderEndTag());
        }
    }
}