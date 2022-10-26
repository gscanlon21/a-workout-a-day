using Microsoft.AspNetCore.Mvc;
using FinerFettle.Web.Data;
using Microsoft.AspNetCore.Authorization;

namespace FinerFettle.Web.Controllers;

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
    public JsonResult IsUserAvailable(string email)
    {
        return Json(!_context.Users.Any(u => u.Email == email.Trim()));
    }
}
