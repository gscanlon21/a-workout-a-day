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
    public UserController() { }
}
