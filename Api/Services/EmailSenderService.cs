using Api.Code;
using Azure;
using Core.Models.Options;
using Data.Data;
using Data.Entities.Newsletter;
using Data.Models.Newsletter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Services;

public class EmailSenderService : BackgroundService
{
    private const string From = "newsletter@aworkoutaday.com";

    private readonly IOptions<FeatureSettings> _featureSettings;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMailSender _mailSender;

    public EmailSenderService(IOptions<FeatureSettings> featureSettings, IMailSender mailSender, IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _featureSettings = featureSettings;
        _mailSender = mailSender;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            Console.WriteLine($"Starting email sender service: {_featureSettings.Value.SendEmail}");

            while (_featureSettings.Value.SendEmail && !stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<CoreContext>();

                UserNewsletter? nextNewsletter = null;
                try
                {
                    // Not worried about repeat reads, only 1 thread.
                    nextNewsletter = await context.UserNewsletters
                        .Include(un => un.User)
                        .OrderBy(un => un.Id)
                        .Where(un => DateTime.UtcNow > un.SendAfter)
                        .Where(un => un.EmailStatus == EmailStatus.Pending)
                        .FirstOrDefaultAsync(CancellationToken.None);

                    if (nextNewsletter != null)
                    {
                        nextNewsletter.SendAttempts += 1;
                        nextNewsletter.EmailStatus = EmailStatus.Sending;
                        context.UserNewsletters.Update(nextNewsletter);
                        await context.SaveChangesAsync(CancellationToken.None);

                        await _mailSender.SendMail(From, nextNewsletter.User.Email, nextNewsletter.Subject, nextNewsletter.Body, CancellationToken.None);

                        nextNewsletter.EmailStatus = EmailStatus.Sent;
                        context.UserNewsletters.Update(nextNewsletter);
                        await context.SaveChangesAsync(CancellationToken.None);
                    }
                    else
                    {
                        // There is no mail to send, wait a minute before retrying
                        await Task.Delay(60000, stoppingToken);
                    }
                }
                catch (Exception e) when (nextNewsletter != null)
                {
                    nextNewsletter.LastError = e.ToString();
                    nextNewsletter.EmailStatus = EmailStatus.Failed;

                    if (e is RequestFailedException requestFailedException)
                    {
                        // If the email soft-bounced after the first try, retry.
                        if (nextNewsletter.SendAttempts <= 1 && requestFailedException.ErrorCode?.StartsWith("5") != true)
                        {
                            nextNewsletter.SendAfter = DateTime.UtcNow.AddHours(1);
                            nextNewsletter.EmailStatus = EmailStatus.Pending;
                        }
                    }

                    context.UserNewsletters.Update(nextNewsletter);
                    await context.SaveChangesAsync(CancellationToken.None);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);

                    // Error querying for new mails, wait a minute before retrying
                    await Task.Delay(60000, stoppingToken);
                }
                finally
                {
                    // Don't want to spam the email sending server
                    await Task.Delay(2000, stoppingToken);
                }
            }

            Console.WriteLine($"Stopping email sender service");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }
}
