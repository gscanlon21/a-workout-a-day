using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Linq.Expressions;
using System.Text.Encodings.Web;

namespace Web.Code;

/// <summary>
/// View-specific extensions and helpers.
/// </summary>
public class HtmlHelpers<TModel> : HtmlHelper<TModel>
{
    private readonly ModelExpressionProvider _modelExpressionProvider;

    public HtmlHelpers(IHtmlGenerator generator,
        ICompositeViewEngine viewEngine,
        IModelMetadataProvider metadataProvider,
        IViewBufferScope scope,
        HtmlEncoder htmlEncoder,
        UrlEncoder urlEncoder,
        ModelExpressionProvider expressionProvider)
        : base(generator, viewEngine, metadataProvider, scope, htmlEncoder, urlEncoder, expressionProvider)
    {
        _modelExpressionProvider = expressionProvider;
    }

    public HtmlString? DisplayDescriptionFor<TProperty>(
        Expression<Func<TModel, TProperty>> expression)
    {
        var metadata = _modelExpressionProvider.CreateModelExpression(ViewData, expression);
        return new HtmlString(metadata.Metadata.Description);
    }
}
