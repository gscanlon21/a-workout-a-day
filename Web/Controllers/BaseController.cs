using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Entities.User;

namespace Web.Controllers;

public class BaseController : Controller
{
    protected readonly CoreContext _context;

    /// <summary>
    /// Returns whether a given uri links to this site
    /// </summary>
    protected bool IsInternalDomain(Uri uri) => string.Equals(uri.Host, "finerfettle.com", StringComparison.OrdinalIgnoreCase)
        || string.Equals(uri.Host, "finerfettle.azurewebsites.net", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Today's date from UTC.
    /// </summary>
    protected static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    public BaseController(CoreContext context)
    {
        _context = context;
    }
}
