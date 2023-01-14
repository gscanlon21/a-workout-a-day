using Microsoft.AspNetCore.Mvc;

namespace Web.Code.Attributes.Response;

public class EnableRouteResponseCompressionAttribute : MiddlewareFilterAttribute
{
    public EnableRouteResponseCompressionAttribute()
        : base(typeof(EnableRouteResponseCompressionAttribute)) { }

    public void Configure(IApplicationBuilder applicationBuilder)
        => applicationBuilder.UseResponseCompression();
}
