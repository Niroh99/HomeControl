using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HomeControl.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("switch", Attributes = "asp-for")]
    public class AspSwitchTagHeper(IHtmlGenerator generator) : InputTagHelper(generator)
    {
        public string CheckedLabel { get; set; } = "On";

        public string UncheckedLabel { get; set; } = "Off";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            SwitchTagHelper.RenderSwitchElements(output, CheckedLabel, UncheckedLabel);
        }
    }
}