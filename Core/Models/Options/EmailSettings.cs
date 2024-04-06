using Core.Code.Attributes;

namespace Core.Models.Options;

/// <summary>
/// App settings for SMTP email sending.
/// </summary>
public class EmailSettings
{
    public EmailType Type { get; init; } = EmailType.None;

    [RequiredIf(nameof(Type), EmailType.Azure)]
    public string CommunicationServicesConnectionString { get; init; } = null!;

    [RequiredIf(nameof(Type), EmailType.SMTP)]
    public int Port { get; init; }

    [RequiredIf(nameof(Type), EmailType.SMTP)]
    public string Server { get; init; } = null!;

    [RequiredIf(nameof(Type), EmailType.SMTP)]
    public string Username { get; init; } = null!;

    [RequiredIf(nameof(Type), EmailType.SMTP)]
    public string Password { get; init; } = null!;

    public enum EmailType
    {
        /// <summary>
        /// Disabled.
        /// </summary>
        None = 0,

        /// <summary>
        /// Use the SMTP email sender.
        /// </summary>
        SMTP = 1,

        /// <summary>
        /// Use the Azure email sender.
        /// </summary>
        Azure = 2
    }
}
