namespace App;

public class DisplayHelper
{
    private readonly AppState appState;

    public DisplayHelper(AppState appState)
    {
        this.appState = appState;
    }

    /// <summary>
    /// Used in the newsletter since relative links won't work in emails.
    /// </summary>
    /// <param name="contentPath">The relative path to the content asset. After wwwroot.</param>
    /// <returns>An absolute uri string to the content asset</returns>
    public string AbsoluteContent(string contentPath)
    {
        return new Uri(new Uri("https://aworkoutaday.com"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", contentPath)).ToString();
    }

    /// <summary>
    /// Generate a link to update tha user's LastActive date and then redirect to the desired url.
    /// </summary>
    public string? StillHereLink(string email, string token, string? to = null)
    {
        return to;
        //return url.ActionLink(nameof(UserController.IAmStillHere), UserController.Name, new { email, token, to });
    }

    /// <summary>
    /// Generate a link to update tha user's LastActive date and then redirect to the desired url.
    /// </summary>
    public string? ActionLink(string actionName, string controllerName, object? parameters = null)
    {
        return "";
        //return url.ActionLink(nameof(UserController.IAmStillHere), UserController.Name, new { email, token, to });
    }
}