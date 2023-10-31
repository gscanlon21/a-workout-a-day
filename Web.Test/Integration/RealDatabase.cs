﻿using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Web.Test.Integration;

public abstract class RealDatabase : FakeDatabase
{
    protected override CoreContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<CoreContext>()
            .UseNpgsql(Config.GetConnectionString("CoreContext"));

        return new CoreContext(optionsBuilder.Options);
    }
}