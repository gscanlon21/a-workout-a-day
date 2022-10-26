using FinerFettle.Functions.Data;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(FinerFettle.Functions.Startup))]
namespace FinerFettle.Functions;
{        
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddDbContext<CoreContext>(options =>
                options.UseNpgsql(Environment.GetEnvironmentVariable("SqlConnectionString") ?? throw new InvalidOperationException("Connection string 'SqlConnectionString' not found.")));
        }
    }
}
