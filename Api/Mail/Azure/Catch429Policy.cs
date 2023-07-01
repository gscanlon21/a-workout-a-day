using Azure;
using Azure.Core;
using Azure.Core.Pipeline;

namespace Api.Mail.Azure;

/// <summary>
/// Throws an exception when email sending tier limit is reached.
/// So Azure doesn't automatically retry the messages, we handle that.
/// 
/// https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/email/send-email-advanced/throw-exception-when-tier-limit-reached?pivots=programming-language-csharp
/// </summary>
public class Catch429Policy : HttpPipelineSynchronousPolicy
{
    public override void OnReceivedResponse(HttpMessage message)
    {
        if (message.Response.Status == 429)
        {
            throw new RequestFailedException(message.Response);
        }
        else
        {
            base.OnReceivedResponse(message);
        }
    }
}