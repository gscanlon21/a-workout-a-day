using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.User.Components;

/// <summary>
/// Viewmodel for UnsupportedClients.cshtml
/// </summary>
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
