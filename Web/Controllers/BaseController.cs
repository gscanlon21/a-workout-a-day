using Microsoft.AspNetCore.Mvc;
using Web.Data;

namespace Web.Controllers;

public class BaseController : Controller
{
    protected readonly CoreContext _context;

    /// <summary>
    /// Today's date from UTC.
    /// </summary>
    protected static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// This week's Sunday date in UTC.
    /// </summary>
    protected static DateOnly StartOfWeek => Today.AddDays(-1 * (int)Today.DayOfWeek);

    public BaseController(CoreContext context)
    {
        _context = context;
    }
}
