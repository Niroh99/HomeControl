using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HomeControl.TagHelpers
{
    [HtmlTargetElement("toggle-button", Attributes = "asp-for", TagStructure = TagStructure.WithoutEndTag)]
    public class AspToggleButtonTagHelper(IHtmlGenerator generator) : InputTagHelper(generator)
    {
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "input";
            output.Attributes.SetAttribute(new TagHelperAttribute("type", "checkbox"));
            output.Attributes.SetAttribute(new TagHelperAttribute("hidden"));

            await ToggleButtonTagHelper.RenderToggleButtonElements(output);
        }
    }
}