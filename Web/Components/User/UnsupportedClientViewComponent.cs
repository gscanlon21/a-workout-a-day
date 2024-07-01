using Microsoft.AspNetCore.Mvc;
using Web.Views.Shared.Components.UnsupportedClient;

namespace Web.Components.User;

public class UnsupportedClientViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "UnsupportedClient";

    public UnsupportedClientViewComponent() { }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        var unsupportedClient = GetUnsupportedClient(user);
        if (unsupportedClient == UnsupportedClientViewModel.UnsupportedClient.None)
        {
            return Content("");
        }

        return View("UnsupportedClient", new UnsupportedClientViewModel()
        {
            Client = unsupportedClient
        });
    }

    internal static UnsupportedClientViewModel.UnsupportedClient GetUnsupportedClient(Data.Entities.User.User user)
    {
        // If the newsletter is disabled. Don't show the unsupported client message.
        if (user.NewsletterDisabledReason != null)
        {
            return UnsupportedClientViewModel.UnsupportedClient.None;
        }

        // Gmail does not support `position:absolute` in emails.
        // https://www.caniemail.com/search/?s=absolute
        if (user.Email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
        {
            return UnsupportedClientViewModel.UnsupportedClient.Gmail;
        }

        return UnsupportedClientViewModel.UnsupportedClient.None;
    }
}
