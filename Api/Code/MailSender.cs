using Core.Models.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Api.Code;

public class MailSender
{
    public const string FromDisplayName = "A Workout a Day";

    private readonly IOptions<SmtpSettings> _smtpSettings;
    private readonly SmtpClient _smtpClient;

    public MailSender(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings;
        _smtpClient = new SmtpClient(_smtpSettings.Value.Server)
        {
            Port = _smtpSettings.Value.Port,
            Credentials = new NetworkCredential(_smtpSettings.Value.Username, _smtpSettings.Value.Password),
            EnableSsl = true,
        };
    }

    public async Task SendMail(string from, string to, string subject, string body)
    {
        var fromAddress = new MailAddress(from, FromDisplayName);
        var toAddress = new MailAddress(to);

        using var mailMessage = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        _smtpClient.Send(mailMessage);
    }
}
