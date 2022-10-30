using Microsoft.AspNetCore.Mvc;

namespace FinerFettle.Web.Attributes.Response;

public class EnableRouteResponseCompressionAttribute : MiddlewareFilterAttribute
{
    public EnableRouteResponseCompressionAttribute()
        : base(typeof(EnableRouteResponseCompressionAttribute)) { }

    public void Configure(IApplicationBuilder applicationBuilder)
        => applicationBuilder.UseResponseCompression();
}
