using App;
using App.Services;
using Core.Models.Options;
using Data.Data;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.IO.Compression;
using Web.Code;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
//builder.Services.AddControllersWithViews();
builder.Services.AddBlazorApp();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddTransient(typeof(HtmlHelpers<>));

builder.Services.AddDbContext<CoreContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CoreContext") ?? throw new InvalidOperationException("Connection string 'CoreContext' not found.")));

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>(); // 1st
    options.Providers.Add<GzipCompressionProvider>(); // fallback
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    // Brotli takes a long time to optimally compress
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<SiteSettings>(
    builder.Configuration.GetSection("SiteSettings")
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

app.MapBlazorHub();

app.MapFallbackToPage("/Error");

app.Run();
