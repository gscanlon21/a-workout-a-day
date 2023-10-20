﻿using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Core.Models.Newsletter;
using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IActionResult> HandleEmailDeliveryReportReceivedPost([FromBody] CloudEvent[] cloudEvents)
    {
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

        var email = await context.UserEmails.FirstOrDefaultAsync(e => e.SenderId == deliveryReport.MessageId);
        if (email == null)
        {
            return;
        }

        if (deliveryReport.Status == AcsEmailDeliveryReportStatus.Delivered)
        {
            email.EmailStatus = EmailStatus.Delivered;
            context.UserEmails.Update(email);
            await context.SaveChangesAsync(CancellationToken.None);
        }
        else
        {
            email.LastError = deliveryReport.Status.ToString();
            email.EmailStatus = EmailStatus.Failed;

            // If the email soft-bounced after the first try, retry.
            if (false)
            {
                email.SendAfter = DateTime.UtcNow.AddHours(1);
                email.EmailStatus = EmailStatus.Pending;
            }

            context.UserEmails.Update(email);
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
