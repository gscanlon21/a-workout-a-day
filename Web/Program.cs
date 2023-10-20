using Core.Code;
using Core.Models.Options;
using Data;
using Data.Repos;
using Lib;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.IO.Compression;
using Web.Code;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddCustomEnvironmentVariables();

// Add services to the container.
builder.Services.AddRazorPages();
// Don't need 
//builder.Services.AddServerSideBlazor();
//builder.Services.AddControllersWithViews();
builder.Services.AddLibServices();
builder.Services.AddHttpClient();

builder.Services.AddTransient<NewsletterRepo>();
builder.Services.AddTransient<UserRepo>();

builder.Services.AddTransient<CaptchaService>();

builder.Services.AddTransient(typeof(HtmlHelpers<>));

builder.Services.AddDbContext<CoreContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CoreContext") ?? throw new InvalidOperationException("Connection string 'CoreContext' not found."),
        b => b.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)
    ));

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

builder.Services.Configure<SiteSettings>(
    builder.Configuration.GetSection("SiteSettings")
);

builder.Services.Configure<CaptchaSettings>(
    builder.Configuration.GetSection("CaptchaSettings")
);

builder.Services.Configure<AzureSettings>(
    builder.Configuration.GetSection("AzureSettings")
);

builder.Services.Configure<FeatureSettings>(
    builder.Configuration.GetSection("FeatureSettings")
);

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
}

app.UseHttpsRedirection();

var staticFilesOptions = new StaticFileOptions()
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

app.MapWhen(context => context.Request.Path.StartsWithSegments("/lib"),
    appBuilder =>
    {
        // Do enable response compression by default for js/css lib files
        appBuilder.UseResponseCompression();

        appBuilder.UseStaticFiles(staticFilesOptions);
    }
);

// Do not enable by default. Controled by a route attribute.
//app.UseResponseCompression();

app.UseStaticFiles();
app.UseStaticFiles(staticFilesOptions);

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllers();

//app.MapBlazorHub();

app.MapFallbackToPage("/Error");

app.Run();
