using Data.Data;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// User helpers.
/// 
/// TODO: User 'forgot password' email. Send them a new token in an email so they can regain access to their account.
/// </summary>
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly CoreContext _context;
    private readonly UserRepo _userRepo;

    public UserController(CoreContext context, UserRepo userRepo)
    {
        _userRepo = userRepo;
        _context = context;
    }
}
