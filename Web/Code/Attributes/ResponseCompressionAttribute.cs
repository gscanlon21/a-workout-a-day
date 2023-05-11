using Microsoft.AspNetCore.Mvc;

namespace Web.Code.Attributes;

/// <summary>
/// Toggle for enabling response compression for a specific route.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ResponseCompressionAttribute : MiddlewareFilterAttribute
{
    public bool Enabled { get; set; } = true;

    public ResponseCompressionAttribute() : base(typeof(ResponseCompressionAttribute)) { }

    public void Configure(IApplicationBuilder applicationBuilder)
    {
        if (Enabled)
        {
            applicationBuilder.UseResponseCompression();
        }
    }
}
