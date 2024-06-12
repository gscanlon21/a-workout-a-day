using Core.Consts;
using Core.Models.User;
using Data;
using Data.Entities.Newsletter;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Api.Code;

public class GlobalExceptionHandler(CoreContext context) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        try
        {
            var adminUsers = await context.Users.Where(u => u.Features.HasFlag(Features.Admin)).ToListAsync(cancellationToken);
            if (adminUsers != null)
            {
                foreach (var adminUser in adminUsers)
                {
                    context.UserEmails.Add(new UserEmail(adminUser)
                    {
                        Subject = NewsletterConsts.SubjectException,
                        Body = exception.Message,
                    });
                }

                await context.SaveChangesAsync(cancellationToken);
            }
        }
        catch { }

        // Return false to continue with the default behavior
        // - or - return true to signal that this exception is handled
        return false;
    }
}