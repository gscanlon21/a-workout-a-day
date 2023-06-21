using Core.Models.Options;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace Api.Jobs;

public class MailSender
{
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
        using var mailMessage = new MailMessage(from, to, subject, body)
        {
            IsBodyHtml = true
        };

        _smtpClient.Send(mailMessage);
    }
}
