using Core.Models.User;
using Lib.ViewModels.User;

namespace App;

public class AppState
{
    public UserNewsletterViewModel? User { get; set; }

    /// <summary>
    /// Should hide detail not shown in the landing page demo?
    /// </summary>
    public bool Demo => User != null && User.Features.HasFlag(Features.Demo);

    /// <summary>
    /// User is null when the exercise is loaded on the site, not in an email newsletter.
    /// 
    /// Emails don't support scripts.
    /// </summary>
    public bool InEmailContext => User != null;
}