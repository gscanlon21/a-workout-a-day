
namespace Core.Models.Options;

/// <summary>
/// App settings for the domain name.
/// </summary>
public class SmtpSettings
{
    public int Port { get; set; }
    public string Server { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}
