using System.ComponentModel.DataAnnotations;

namespace Web.Views.Shared.Components.UnsupportedClient;

public class UnsupportedClientViewModel
{
    public required UnsupportedClient Client { get; init; }

    public enum UnsupportedClient
    {
        None = 0,

        [Display(Name = "Gmail")]
        Gmail = 1,
    }
}
