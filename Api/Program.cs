using Api.Controllers;
using Api.Jobs;
using Api.Jobs.Newsletter;
using Api.Jobs.User;
using Core.Models.Options;
using Data.Data;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Quartz;

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
builder.Services.AddTransient<HttpClient>();
builder.Services.AddTransient<MailSender>();

builder.Services.Configure<SiteSettings>(
    builder.Configuration.GetSection("SiteSettings")
);

builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings")
);

builder.Services.Configure<QuartzOptions>(options =>
{
    options.Scheduling.IgnoreDuplicates = true; // default: false
    options.Scheduling.OverWriteExistingData = true; // default: true
});
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
    q.UsePersistentStore(x => {
        x.UseProperties = true;
        x.UsePostgres(builder.Configuration.GetConnectionString("QuartzContext") ?? throw new InvalidOperationException("Connection string 'CoreContext' not found."));
        x.UseSerializer<Quartz.Simpl.JsonObjectSerializer>();
    });
});
builder.Services.AddQuartzHostedService(opt =>
{
    opt.WaitForJobsToComplete = true;
});

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

var schedulerFactory = app.Services.GetRequiredService<ISchedulerFactory>();
var scheduler = await schedulerFactory.GetScheduler();

await DeleteOldNewsletters.Schedule(scheduler);
await DeleteInactiveUsers.Schedule(scheduler);
await DeleteOldTokens.Schedule(scheduler);
await DisableInactiveUsers.Schedule(scheduler);
await NewsletterJob.Schedule(scheduler);
await NewsletterTestJob.Schedule(scheduler);
await NewsletterDebugJob.Schedule(scheduler);

app.Run();
