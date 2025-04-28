using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HomeControl.TagHelpers
{
    [HtmlTargetElement("switch")]
    public class SwitchTagHelper : TagHelper
    {
        public string CheckedLabel { get; set; } = "On";

        public string UncheckedLabel { get; set; } = "Off";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "input";
            output.Attributes.SetAttribute(new TagHelperAttribute("type", "checkbox"));
            output.Attributes.SetAttribute(new TagHelperAttribute("hidden"));

            RenderSwitchElements(output, CheckedLabel, UncheckedLabel);
        }

        public static void RenderSwitchElements(TagHelperOutput output, string checkedLabelContent, string uncheckedLabelContent)
        {
            var container = new TagBuilder("div");
            container.AddCssClass("switch-container");
            container.AddCssClass("centered-content");

            output.PreElement.AppendHtml(container.RenderStartTag());

            var checkedLabel = new TagBuilder("label");
            checkedLabel.AddCssClass("switch-checked-label");
            checkedLabel.InnerHtml.Append(checkedLabelContent);

            output.PreElement.AppendHtml(checkedLabel);

            var uncheckedLabel = new TagBuilder("label");
            uncheckedLabel.AddCssClass("switch-unchecked-label");
            uncheckedLabel.InnerHtml.Append(uncheckedLabelContent);

            output.PreElement.AppendHtml(uncheckedLabel);

            var inputLabel = new TagBuilder("label");
            inputLabel.AddCssClass("switch-input-label");
            inputLabel.AddCssClass("shadow-2");

            output.PreElement.AppendHtml(inputLabel.RenderStartTag());

            var switchThumb = new TagBuilder("span");
            switchThumb.AddCssClass("switch-thumb");

            output.PostElement.AppendHtml(switchThumb);

            output.PostElement.AppendHtml(inputLabel.RenderEndTag());
            output.PostElement.AppendHtml(container.RenderEndTag());
        }
    }
}