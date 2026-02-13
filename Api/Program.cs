using ADay.Data.Code.Extensions;
using Api.Code;
using Api.Controllers;
using Api.Jobs.Create;
using Api.Jobs.Delete;
using Api.Jobs.Update;
using Api.Mail.Azure;
using Api.Mail.Smtp;
using Api.Services;
using Core.Models.Options;
using Data;
using Data.Repos;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddCustomEnvironmentVariables();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddShared(builder.Configuration, migrations: false);
builder.Services.AddDbContext<CoreContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CoreContext") ?? throw new InvalidOperationException("Connection string 'CoreContext' not found."), options =>
    {
        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
    }));

builder.Services.AddTransient<UserController>();
builder.Services.AddTransient<NewsletterController>();

builder.Services.AddTransient<UserRepo>();
builder.Services.AddTransient<NewsletterRepo>();

builder.Services.AddHostedService<EmailSenderService>();
switch (builder.Configuration.GetSection("EmailSettings").GetValue<EmailSettings.EmailType>("Type"))
{
    case EmailSettings.EmailType.SMTP:
        builder.Services.AddSingleton<IMailSender, SmtpMailSender>();
        break;
    case EmailSettings.EmailType.Azure:
        builder.Services.AddSingleton<IMailSender, AzureMailSender>();
        break;
    default:
        builder.Services.AddSingleton<IMailSender>(c => null!);
        break;
}

builder.Services.AddOptions<EmailSettings>()
    .Bind(builder.Configuration.GetSection("EmailSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<SiteSettings>()
    .Bind(builder.Configuration.GetRequiredSection("SiteSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.Configure<QuartzOptions>(options =>
{
    options.Scheduling.IgnoreDuplicates = true;
    options.Scheduling.OverWriteExistingData = true;
});
builder.Services.AddQuartz(q =>
{
    if (DebugConsts.IsDebug)
    {
        q.UseInMemoryStore();
    }
    else
    {
        q.UsePersistentStore(x =>
        {
            x.UseProperties = true;
            x.UsePostgres(builder.Configuration.GetConnectionString("QuartzContext") ?? throw new InvalidOperationException("Connection string 'CoreContext' not found."));
            x.UseSerializer<Quartz.Simpl.JsonObjectSerializer>();
        });
    }
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

await CreateEmails.Schedule(scheduler);
await CreateWorkouts.Schedule(scheduler);
await CreateBackfill.Schedule(scheduler);
await DisableInactiveUsers.Schedule(scheduler);
await DisableErroredUsers.Schedule(scheduler);
await DeleteInactiveUsers.Schedule(scheduler);
await DeleteOldWorkouts.Schedule(scheduler);
await DeleteOldEmails.Schedule(scheduler);
await DeleteOldTokens.Schedule(scheduler);
await DeleteOldLogs.Schedule(scheduler);

app.Run();
