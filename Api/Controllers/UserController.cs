using Core.Consts;
using Data.Entities.Newsletter;
using Data.Entities.User;
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
    public async Task<User?> GetUser(string email = UserConsts.DemoUser, string token = UserConsts.DemoToken)
    {
        return await userRepo.GetUser(email, token);
    }

    /// <summary>
    /// Get the user's past workouts.
    /// </summary>
    [HttpGet("Workouts")]
    public async Task<IList<UserWorkout>?> GetWorkouts(string email = UserConsts.DemoUser, string token = UserConsts.DemoToken)
    {
        var user = await userRepo.GetUser(email, token);
        if (user == null)
        {
            return null;
        }

        return await userRepo.GetPastWorkouts(user);
    }
}
