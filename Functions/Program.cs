using FinerFettle.Functions.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(s =>
    {
        s.AddDbContext<CoreContext>(options =>
            options.UseNpgsql(Environment.GetEnvironmentVariable("SqlConnectionString") ?? throw new InvalidOperationException("Connection string 'SqlConnectionString' not found.")));
    })
    .Build();

host.Run();

