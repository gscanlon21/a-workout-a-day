using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web.Code.TagHelpers;

/// <summary>
/// Range slider element with an output display.
/// </summary>
[HtmlTargetElement("output-range", Attributes = "asp-for")]
public class OutputRangeTagHelper : TagHelper
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

        var outputName = $"{id}Output";
        output.Content.SetHtmlContent($"""
            <output name="{outputName}" for="{id}" class="text-end" style="min-width:3ch;">{For.Model}</output>
            <div class="d-flex w-100">
                <input type="range"
                        id="{id}"
                        name="{name}"
                        value="{For.Model}"
                        min="{Min}"
                        max="{Max}"
                        step="{Step}"
                        class="w-100"
                        oninput="{outputName}.value = this.valueAsNumber" />
            </div>
            """);
    }
}