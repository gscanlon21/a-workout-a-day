﻿using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Api;

[ApiController]
public class ApiController : ControllerBase
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    protected static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// This week's Sunday date in UTC.
    /// </summary>
    protected static DateOnly StartOfWeek => Today.AddDays(-1 * (int)Today.DayOfWeek);

    public ApiController() { }
}
