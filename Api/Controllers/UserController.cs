using Core.Code.Exceptions;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// User helpers.
/// </summary>
[ApiController, Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly UserRepo _userRepo;

    public UserController(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    /// <summary>
    /// Get the user.
    /// </summary>
    [HttpGet("User")]
    public async Task<IActionResult> GetUser(string email = UserConsts.DemoUser, string token = UserConsts.DemoToken)
    {
        try
        {
            var user = await _userRepo.GetUserStrict(email, token);
            return StatusCode(StatusCodes.Status200OK, user);
        }
        catch (UserException)
        {
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Log an exception.
    /// </summary>
    [HttpPost("LogException")]
    public async Task LogException([FromForm] string? email, [FromForm] string token, [FromForm] string? message)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        throw new Exception(message);
    }
}
