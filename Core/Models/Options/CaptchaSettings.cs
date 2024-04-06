using Core.Code.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Core.Models.Options;

/// <summary>
/// App settings for the captcha service.
/// </summary>
public class CaptchaSettings
{
    /// <summary>
    /// Is captcha enabled?
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// The public site-key for the captcha.
    /// </summary>
    [RequiredIf(nameof(Enabled), true)]
    public string SiteKey { get; init; } = null!;

    /// <summary>
    /// The secret api-key for the captcha.
    /// </summary>
    [RequiredIf(nameof(Enabled), true)]
    public string ApiKey { get; init; } = null!;

    /// <summary>
    /// The link to the captcha verify api.
    /// 
    /// sa. https://api.friendlycaptcha.com/api/v1/siteverify
    /// </summary>
    [RequiredIf(nameof(Enabled), true)]
    public string ApiLink { get; init; } = null!;
    public Uri ApiUri => ApiLink != null ? new(ApiLink) : null!;
}
