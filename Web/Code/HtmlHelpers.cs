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
    ModelExpressionProvider expressionProvider) : HtmlHelper<TModel>(generator, viewEngine, metadataProvider, scope, htmlEncoder, urlEncoder, expressionProvider)
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
