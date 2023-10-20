using Api.Code;
using Azure;
using Azure.Communication.Email;
using Azure.Core;
using Core.Models.Options;
using Microsoft.Extensions.Options;

namespace Api.Mail.Azure;

/// <summary>
/// Sending limits: https://learn.microsoft.com/en-us/azure/communication-services/concepts/service-limits#email
/// </summary>
public class AzureMailSender : IMailSender
{
    public const string FromDisplayName = "A Workout a Day";

    private readonly EmailClient _emailClient;

    public AzureMailSender(IOptions<AzureSettings> azureSettings)
    {
        var emailClientOptions = new EmailClientOptions();
        emailClientOptions.AddPolicy(new Catch429Policy(), HttpPipelinePosition.PerRetry);

        _emailClient = new EmailClient(azureSettings.Value.CommunicationServicesConnectionString, emailClientOptions);
    }

    public async Task<string?> SendMail(string from, string to, string subject, string body, CancellationToken cancellationToken)
    {
        var content = new EmailContent(subject)
        {
            Html = body
        };

        var message = new EmailMessage(from, to, content);
        return (await _emailClient.SendAsync(WaitUntil.Completed, message, cancellationToken)).Id;
    }
}
