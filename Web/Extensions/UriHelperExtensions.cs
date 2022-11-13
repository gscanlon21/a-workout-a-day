using Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Web.Extensions;

public static class UrlHelperExtensions
{
    /// <summary>
    /// Used in the newsletter since relative links won't work in emails.
    /// </summary>
    /// <param name="contentPath">The relative path to the content asset. After wwwroot.</param>
    /// <returns>An absolute uri string to the content asset</returns>
    public static string AbsoluteContent(this IUrlHelper url, string contentPath)
    {
        HttpRequest request = url.ActionContext.HttpContext.Request;
        return new Uri(new Uri(request.Scheme + "://" + request.Host.Value), url.Content(contentPath)).ToString();
    }

    /// <summary>
    /// Generate a link to update tha user's LastActive date and then redirect to the desired url.
    /// </summary>
    public static string? StillHereLink(this IUrlHelper url, string email, string token, string? redirectTo = null)
    {
        return url.ActionLink(nameof(UserController.IAmStillHere), UserController.Name, new { email, token, redirectTo });
    }
}
