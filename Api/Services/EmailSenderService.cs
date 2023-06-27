using Api.Code;
using Core.Models.Options;
using Data.Data;
using Data.Entities.Newsletter;
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
                    nextNewsletter = context.UserNewsletters
                        .Include(un => un.User)
                        .FirstOrDefault(un => !un.Sent && un.Error == null);

                    if (nextNewsletter != null)
                    {
                        await _mailSender.SendMail(From, nextNewsletter.User.Email, nextNewsletter.Subject, nextNewsletter.Body, stoppingToken);

                        nextNewsletter.Sent = true;
                        context.UserNewsletters.Update(nextNewsletter);
                        await context.SaveChangesAsync(stoppingToken);
                    }
                    else
                    {
                        // There is no mail to send, wait a minute before retrying
                        await Task.Delay(60000, stoppingToken);
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);

                    if (nextNewsletter != null)
                    {
                        nextNewsletter.Error = e.ToString();
                        context.UserNewsletters.Update(nextNewsletter);
                        await context.SaveChangesAsync(stoppingToken);
                    }
                    else
                    {
                        // Error querying for new mails, wait a minute before retrying
                        await Task.Delay(30000, stoppingToken);
                    }
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
