using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public class ViewController : Controller
{
    public const string EmailRegex = @"\s*\S+@\S+\.\S+\s*";
    public const string EmailRegexError = "Please enter a valid email address.";

    public const string UserRoute = $"{{email:regex({EmailRegex})}}";

    /// <summary>
    /// Message to show to the user when a link has expired.
    /// </summary>
    public const string LinkExpiredMessage = "This link has expired.";
}
