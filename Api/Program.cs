using Api.Controllers;
using Data.Data;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<CoreContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CoreContext") ?? throw new InvalidOperationException("Connection string 'CoreContext' not found.")));

builder.Services.AddTransient<UserController>();
builder.Services.AddTransient<NewsletterController>();
builder.Services.AddTransient<FootnoteController>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});

/*
app.Use((context, next) =>
{
    context.Request.Scheme = "https";
    return next(context);
});
*/

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
