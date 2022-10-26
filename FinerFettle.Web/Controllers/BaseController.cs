using Microsoft.AspNetCore.Mvc;
using FinerFettle.Web.Data;

namespace FinerFettle.Web.Controllers;

public class BaseController : Controller
{
    protected readonly CoreContext _context;

    /// <summary>
    /// Returns whether a given uri links to this site
    /// </summary>
    protected bool IsInternalDomain(Uri uri) => string.Equals(uri.Host, "finerfettle.com", StringComparison.OrdinalIgnoreCase)
        || string.Equals(uri.Host, "finerfettle.azurewebsites.net", StringComparison.OrdinalIgnoreCase);

    public BaseController(CoreContext context)
    {
        _context = context;
    }
}
