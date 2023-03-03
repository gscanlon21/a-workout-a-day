using Microsoft.AspNetCore.Mvc;

namespace Web.Code.Attributes.Response;

/// <summary>
/// Toggle for enabling response compression for a specific route.
/// </summary>
public class EnableRouteResponseCompressionAttribute : MiddlewareFilterAttribute
{
    public EnableRouteResponseCompressionAttribute()
        : base(typeof(EnableRouteResponseCompressionAttribute)) { }

    public void Configure(IApplicationBuilder applicationBuilder)
        => applicationBuilder.UseResponseCompression();
}
