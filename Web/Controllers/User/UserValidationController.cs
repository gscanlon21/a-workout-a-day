using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Web.Data;
using Web.Models.Options;

namespace Web.Controllers.User;

[Route("user/validate")]
public class UserValidationController : BaseController
{
    /// <summary>
    /// The name of the controller for routing purposes
    /// </summary>
    public const string Name = "UserValidation";

    private readonly CoreContext _context;
    private readonly IOptions<SiteSettings> _siteSettings;

    public UserValidationController(CoreContext context, IOptions<SiteSettings> siteSettings) : base()
    {
        _context = context;
        _siteSettings = siteSettings;
    }

    /// <summary>
    /// Validation route for whether a user already exists in the database
    /// </summary>
    [AllowAnonymous, Route("email")]
    public async Task<JsonResult> IsUserAvailable(string email)
    {
        email = email.Trim();

        // Don't let users signup as our domain.
        if (email.Contains(_siteSettings.Value.Domain, StringComparison.OrdinalIgnoreCase))
        {
            return Json("Invalid email.");
        }

        // Gmail does not support position:absolute.
        // https://www.caniemail.com/search/?s=absolute 
        var validGmailEnding = $"+{_siteSettings.Value.ApexDomainSansTLD}@gmail.com";
        if (email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase)
            && !email.EndsWith(validGmailEnding, StringComparison.OrdinalIgnoreCase))
        {
            var splitEmail = email.Split('+', '@');
            // Fragmented; muddled; diorganized; disjointed; jumbled.
            return Json($"Gmail is not a supported email client. Emails may appear garbled. If you understand, use the email: {splitEmail[0] + validGmailEnding}");
        }

        // The same user is already signed up.
        if (await _context.Users.AnyAsync(u => EF.Functions.ILike(u.Email, email)))
        {
            return Json("Manage your preferences from the previous newsletter.");
        }

        return Json(true);
    }
}
