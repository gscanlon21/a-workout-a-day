using Core.Models.Options;
using Data;
using Data.Repos;
using Lib;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.IO.Compression;
using Web.Code;
using Web.Code.RouteConstraints;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddCustomEnvironmentVariables();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Disable the anti-forgery cookie. We don't authenticate using cookies.
builder.Services.AddAntiforgery(options => { options.Cookie.Expiration = TimeSpan.Zero; });
builder.Services.AddRazorPages(options =>
{
    // Ignore anti-forgery tokens by default. We don't authenticate using cookies.
    // https://security.stackexchange.com/questions/62080/is-csrf-possible-if-i-dont-even-use-cookies
    options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
});

// Don't need.
//builder.Services.AddServerSideBlazor();
//builder.Services.AddControllersWithViews();

builder.Services.AddLibServices();
builder.Services.AddHttpClient();

builder.Services.AddTransient<NewsletterRepo>();
builder.Services.AddTransient<UserRepo>();

builder.Services.AddTransient<CaptchaService>();

builder.Services.AddTransient(typeof(HtmlHelpers<>));

builder.Services.AddDbContext<CoreContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CoreContext") ?? throw new InvalidOperationException("Connection string 'CoreContext' not found."), options =>
    {
        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
        options.MigrationsAssembly(typeof(CoreContext).Assembly.GetName().Name);
    }));

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>(); // 1st
    options.Providers.Add<GzipCompressionProvider>(); // fallback
});

builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap.Add(SectionRouteConstraint.Name, typeof(SectionRouteConstraint));
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    // Brotli takes a long time to optimally compress
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddOptions<SiteSettings>()
    .Bind(builder.Configuration.GetRequiredSection("SiteSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<CaptchaSettings>()
    .Bind(builder.Configuration.GetSection("CaptchaSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Necessary for isolation scoped css
builder.WebHost.UseStaticWebAssets();

// See https://aka.ms/aspnetcore-hsts.
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

var staticFileOptions = new StaticFileOptions()
{
    OnPrepareResponse = (context) =>
    {
        context.Context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
        {
            Public = true,
            MaxAge = TimeSpan.FromDays(365)
        };
    }
};

app.MapWhen(context => context.Request.Path.StartsWithSegments("/lib"), appBuilder =>
{
    // Do enable response compression by default for js/css lib files.
    // It is controlled by a route attribute.
    appBuilder.UseResponseCompression();

    // Need this in the conditional or it doesn't apply.
    appBuilder.UseStaticFiles(staticFileOptions);
});

// Map this after the conditional.
app.UseStaticFiles(staticFileOptions);

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllers();

//app.MapBlazorHub();

app.MapFallbackToPage("/404");

app.Run();
