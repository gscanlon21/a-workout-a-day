using Azure;
using Azure.Communication.Email;
using Core.Models.Options;
using Microsoft.Extensions.Options;

namespace Api.Code;

/// <summary>
/// Sending limits: https://learn.microsoft.com/en-us/azure/communication-services/concepts/service-limits#email
/// </summary>
public class AzureMailSender : IMailSender
{
    public const string FromDisplayName = "A Workout a Day";

    private readonly EmailClient _emailClient;

    public AzureMailSender(IOptions<AzureSettings> azureSettings)
    {
        _emailClient = new EmailClient(azureSettings.Value.CommunicationServicesConnectionString);
    }

    public async Task SendMail(string from, string to, string subject, string body, CancellationToken cancellationToken)
    {
        var content = new EmailContent(subject)
        {
            Html = body
        };

        var message = new EmailMessage(from, to, content);
        await _emailClient.SendAsync(WaitUntil.Completed, message, cancellationToken);
    }
}
