using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using System.Linq.Expressions;
using System.Text.Encodings.Web;

namespace Web.Code;

/// <summary>
/// View-specific extensions and helpers.
/// </summary>
public class HtmlHelpers<TModel>(IHtmlGenerator generator,
    ICompositeViewEngine viewEngine,
    IModelMetadataProvider metadataProvider,
    IViewBufferScope scope,
    HtmlEncoder htmlEncoder,
    UrlEncoder urlEncoder,
#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
    ModelExpressionProvider expressionProvider) : HtmlHelper<TModel>(generator, viewEngine, metadataProvider, scope, htmlEncoder, urlEncoder, expressionProvider)
#pragma warning restore CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
{

    /// <summary>
    /// Returns the Description property of the Display attribute for the model property.
    /// </summary>
    public HtmlString? DisplayDescriptionFor<TProperty>(
        Expression<Func<TModel, TProperty>> expression)
    {
        var metadata = expressionProvider.CreateModelExpression(ViewData, expression);
        return new HtmlString(metadata.Metadata.Description);
    }
}
