using Core.Code.Exceptions;
using Core.Consts;
using Core.Models.User;
using Data;
using Data.Entities.Newsletter;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Api.Code;

public class GlobalExceptionHandler(IServiceScopeFactory serviceScopeFactory) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        try
        {
            // Don't send exception emails when debugging.
            if (!DebugConsts.IsDebug
                // Don't send exception emails for user access errors.
                && exception is not UserException
                // Don't send exception emails for transient exceptions.
                && !exception.Message.Contains("transient failure", StringComparison.OrdinalIgnoreCase))
            {
                await SendExceptionEmails(exception, cancellationToken);
            }
        }
        catch { }

        // Return false to continue with the default behavior
        // - or - return true to signal that this exception is handled
        return false;
    }

    private async Task SendExceptionEmails(Exception exception, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<CoreContext>();

        // Send just one a day.
        var oneSentToday = await context.UserEmails.Where(ue => ue.Date == DateHelpers.Today && ue.Subject == EmailConsts.SubjectException).AnyAsync(cancellationToken);
        if (!oneSentToday)
        {
            var debugUsers = await context.Users.Where(u => u.Features.HasFlag(Features.Debug)).ToListAsync(cancellationToken);
            if (debugUsers != null)
            {
                foreach (var debugUser in debugUsers)
                {
                    context.UserEmails.Add(new UserEmail(debugUser)
                    {
                        Subject = EmailConsts.SubjectException,
                        Body = $"<pre>{exception}</pre>",
                    });
                }

                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}