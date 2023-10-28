using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User.Components;

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
        var unsupportedCient = GetUnsupportedClient(user);
        if (unsupportedCient == UnsupportedClientViewModel.UnsupportedClient.None)
        {
            return Content("");
        }

        return View("UnsupportedClient", new UnsupportedClientViewModel()
        {
            Client = unsupportedCient
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
