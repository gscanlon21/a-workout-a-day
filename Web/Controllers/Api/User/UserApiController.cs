using App.Services;
using Core.Models.Options;
using Data.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Web.Controllers.Api.User;

[Route("api/user", Order = 1)]
[Route("api/u", Order = 2)]
public class UserApiController : ApiController
{
    /// <summary>
    /// The name of the controller for routing purposes
    /// </summary>
    public const string Name = "UserApi";

    private readonly CoreContext _context;
    private readonly UserService _userService;
    private readonly IOptions<SiteSettings> _siteSettings;

    public UserApiController(CoreContext context, UserService userService, IOptions<SiteSettings> siteSettings) : base()
    {
        _userService = userService;
        _context = context;
        _siteSettings = siteSettings;
    }

    [AllowAnonymous, Route("token")]
    public ContentResult Token()
    {
        return Content(_userService.CreateToken());
    }

    /// <summary>
    /// Validation route for whether a user already exists in the database
    /// </summary>
    [AllowAnonymous, Route("availability")]
    public async Task<JsonResult> IsUserAvailable(string email)
    {
        email = email.Trim();

        // Don't let users signup as our domain.
        if (email.Contains(_siteSettings.Value.Domain, StringComparison.OrdinalIgnoreCase))
        {
            return new JsonResult("Invalid email.");
        }

        // Gmail does not support position:absolute.
        // https://www.caniemail.com/search/?s=absolute 
        var validGmailEnding = $"+{_siteSettings.Value.ApexDomainSansTLD}@gmail.com";
        if (email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase)
            && !email.EndsWith(validGmailEnding, StringComparison.OrdinalIgnoreCase))
        {
            var splitEmail = email.Split('+', '@');
            // Fragmented; muddled; diorganized; disjointed; jumbled.
            return new JsonResult($"Gmail is not a supported email client. Emails may appear garbled. If you understand, use the email: {splitEmail[0] + validGmailEnding}");
        }

        // The same user is already signed up.
        if (await _context.Users.AnyAsync(u => EF.Functions.ILike(u.Email, email)))
        {
            return new JsonResult("Invalid email.");
        }

        return new JsonResult(true);
    }
}
