using Core.Consts;
using Core.Models.User;
using Data;
using Data.Entities.Newsletter;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Web.Code;

public class GlobalExceptionHandler(IServiceScopeFactory serviceScopeFactory) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!DebugConsts.IsDebug)
            {
                using var scope = serviceScopeFactory.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<CoreContext>();

                var devUsers = await context.Users.Where(u => u.Features.HasFlag(Features.Dev)).ToListAsync(cancellationToken);
                if (devUsers != null)
                {
                    foreach (var devUser in devUsers)
                    {
                        context.UserEmails.Add(new UserEmail(devUser)
                        {
                            Subject = NewsletterConsts.SubjectException,
                            Body = exception.Message,
                        });
                    }

                    await context.SaveChangesAsync(cancellationToken);
                }
            }
        }
        catch { }

        // Return false to continue with the default behavior
        // - or - return true to signal that this exception is handled
        return false;
    }
}