using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Core.Consts;
using Core.Models.Newsletter;
using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AzureController(ILogger<AzureController> logger, CoreContext context, IHttpClientFactory httpClientFactory) : ControllerBase
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    [HttpOptions("EmailEventsSubscription")]
    public async Task<IActionResult> HandleEmailDeliveryReportReceived()
    {
        var webhookcallback = Request.Headers.GetCommaSeparatedValues("WebHook-Request-Callback")?.FirstOrDefault()?.Trim();
        if (string.IsNullOrEmpty(webhookcallback) == false)
        {
            // Validation has to be completed after the OPTIONS request is reponded to.
            ThreadPool.QueueUserWorkItem(async (object? state) =>
            {
                await Task.Delay(5000);
                var result = await _httpClient.GetAsync(webhookcallback).Result.Content.ReadAsStringAsync();
                logger.Log(LogLevel.Information, "{result}", result);
            });

            // Respond to OPTIONS
            Response.Headers.Append("WebHook-Request-Origin", "eventgrid.azure.net");
            Response.Headers.Append("WebHook-Allowed-Rate", "120");
            return Ok();
        }

        return BadRequest();
    }

    [HttpPost("EmailEventsSubscription")]
    public async Task<IActionResult> HandleEmailDeliveryReportReceivedPost([FromBody] JsonNode content)
    {
        var cloudEvents = content switch
        {
            _ when content is JsonArray => content.Deserialize<CloudEvent[]>(),
            _ when content is JsonObject => content.Deserialize<CloudEvent>() is CloudEvent cloudEvent ? [cloudEvent] : null,
            _ => null
        };

        if (cloudEvents == null)
        {
            return BadRequest();
        }

        foreach (CloudEvent cloudEvent in cloudEvents.Where(e => e.Data != null))
        {
            switch (cloudEvent.Type)
            {
                case SystemEventNames.AcsEmailDeliveryReportReceived:
                    await HandleDeliveryReport(cloudEvent.Data!.ToObjectFromJson<AcsEmailDeliveryReportReceivedEventData>());
                    break;
            }
        }

        return Ok();
    }

    private async Task HandleDeliveryReport(AcsEmailDeliveryReportReceivedEventData deliveryReport)
    {
        if (!deliveryReport.Status.HasValue)
        {
            return;
        }

        var email = await context.UserEmails
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.SenderId == deliveryReport.MessageId);

        if (email == null)
        {
            return;
        }

        if (deliveryReport.Status == AcsEmailDeliveryReportStatus.Delivered)
        {
            email.EmailStatus = EmailStatus.Delivered;
            await context.SaveChangesAsync(CancellationToken.None);
        }
        else
        {
            email.EmailStatus = EmailStatus.Failed;
            email.LastError = deliveryReport.DeliveryStatusDetails.StatusMessage;

            // If the email soft-bounced after the first try, retry.
            if (email.SendAttempts <= NewsletterConsts.MaxSendAttempts && deliveryReport.Status == AcsEmailDeliveryReportStatus.Failed)
            {
                email.SendAfter = DateTime.UtcNow.AddHours(1);
                email.EmailStatus = EmailStatus.Pending;
            }
            // If the newsletter failed, disable it.
            else if (email.Subject == NewsletterConsts.SubjectWorkout)
            {
                email.User.NewsletterDisabledReason = $"Email failed with status: {deliveryReport.Status}.";
            }

            await context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
