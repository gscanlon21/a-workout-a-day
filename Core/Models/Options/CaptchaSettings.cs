
namespace Core.Models.Options;

/// <summary>
/// App settings for the domain name.
/// </summary>
public class CaptchaSettings
{
    /// <summary>
    /// The public site-key for the captcha.
    /// </summary>
    public string SiteKey { get; set; } = null!;

    /// <summary>
    /// The secret api-key for the captcha.
    /// </summary>
    public string ApiKey { get; set; } = null!;

    /// <summary>
    /// The link to the captcha verify api.
    /// 
    /// sa. https://api.friendlycaptcha.com/api/v1/siteverify
    /// </summary>
    public string ApiLink { get; set; } = null!;
    public Uri ApiUri => new(ApiLink);
}
