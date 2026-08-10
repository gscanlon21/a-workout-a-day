using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web.Code.TagHelpers;

/// <summary>
/// Range slider element with an input textbox.
/// </summary>
[HtmlTargetElement("input-range", Attributes = "asp-for")]
public class InputRangeTagHelper : TagHelper
{
    [HtmlAttributeName("asp-for")]
    public ModelExpression For { get; set; } = null!;

    [HtmlAttributeName("step")]
    public double Step { get; set; } = 1;

    [HtmlAttributeName("min")]
    public double Min { get; set; } = 0;

    [HtmlAttributeName("max")]
    public double Max { get; set; } = 100;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var name = For.Name;
        var id = TagBuilder.CreateSanitizedId(name, "_");

        output.TagName = "div";
        output.Attributes.SetAttribute("class", "d-flex column-gap-2");

        output.Content.SetHtmlContent($"""
            <input type="number"
                    id="{id}"
                    name="{name}"
                    value="{For.Model}"
                    style="width:6ch;height:fit-content;"
                    oninput="{id}Slider.value = this.valueAsNumber" />

            <div class="d-flex w-100 flex-column justify-content-center">
                <input type="range"
                        id="{id}-slider"
                        name="{id}Slider"
                        min="{Min}"
                        max="{Max}"
                        step="{Step}"
                        value="{For.Model}"
                        class="w-100"
                        oninput="{name}.value = this.value" />
            </div>
            """);
    }
}