using Core.Consts;
using Data.Entities.Newsletter;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// User helpers.
/// </summary>
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly UserRepo _userRepo;

    public UserController(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter.
    /// </summary>
    [HttpGet("Workouts")]
    public async Task<IList<UserWorkout>?> Workouts(string email = UserConsts.DemoUser, string token = UserConsts.DemoToken)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return null;
        }

        return await _userRepo.GetPastWorkouts(user);
    }
}
