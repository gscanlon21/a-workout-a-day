using Core.Code.Exceptions;
using Core.Consts;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// User helpers.
/// </summary>
[ApiController]
[Route("[controller]")]
public class UserController(UserRepo userRepo) : ControllerBase
{
    /// <summary>
    /// Get the user.
    /// </summary>
    [HttpGet("User")]
    public async Task<IActionResult> GetUser(string email = UserConsts.DemoUser, string token = UserConsts.DemoToken)
    {
        try
        {
            var user = await userRepo.GetUserStrict(email, token);
            return StatusCode(StatusCodes.Status200OK, user);
        }
        catch (UserException)
        {
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Get the user's past workouts.
    /// </summary>
    [HttpGet("Workouts")]
    public async Task<IActionResult> GetWorkouts(string email = UserConsts.DemoUser, string token = UserConsts.DemoToken)
    {
        try
        {
            var user = await userRepo.GetUserStrict(email, token);
            var workouts = await userRepo.GetPastWorkouts(user);
            if (workouts != null)
            {
                return StatusCode(StatusCodes.Status200OK, workouts);
            }

            return StatusCode(StatusCodes.Status204NoContent);
        }
        catch (UserException)
        {
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Get the user's past workouts.
    /// </summary>
    [HttpPost("LogException")]
    public async Task LogException([FromForm] string? email, [FromForm] string token, [FromForm] string? message)
    {
        var user = await userRepo.GetUser(email, token);
        if (user == null || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        throw new Exception(message);
    }
}
