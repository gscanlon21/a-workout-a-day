using FinerFettle.Web.Data;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

var staticFilesOptions = new StaticFileOptions()
{
    OnPrepareResponse = (context) =>
    {
        context.Context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
        {
            Public = true,
            MaxAge = TimeSpan.FromDays(30)
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

app.UseStaticFiles(staticFilesOptions);

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllers();

app.Run();
