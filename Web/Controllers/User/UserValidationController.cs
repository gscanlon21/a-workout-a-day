using Microsoft.AspNetCore.Mvc;
using Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers.User;

[Route("user/validate")]
public class UserValidationController : BaseController
{
    /// <summary>
    /// The name of the controller for routing purposes
    /// </summary>
    public const string Name = "UserValidation";

    public UserValidationController(CoreContext context) : base(context) { }

    /// <summary>
    /// Validation route for whether a user already exists in the database
    /// </summary>
    [AllowAnonymous, Route("email")]
    public async Task<JsonResult> IsUserAvailable(string email)
    {
        return Json(!await _context.Users.AnyAsync(u => EF.Functions.ILike(u.Email, email.Trim())));
    }
}
