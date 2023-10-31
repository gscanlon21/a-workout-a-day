using Api.Services;
using Data.Entities.User;
using Data.Test.Code;

namespace Api.Test.Unit.Services;

[TestClass]
public class TestEmailSenderService : FakeDatabase
{
    [TestMethod]
    public async Task GetNextNewsletter_WhenDateIsRecent_ReturnsOne()
    {
        var user = new User(string.Empty, true, false);
        Context.UserEmails.Add(new Data.Entities.Newsletter.UserEmail(user)
        {
            Body = string.Empty,
            Subject = string.Empty,
            Date = Today,
            SendAfter = DateTime.UtcNow.AddMinutes(-1),
        });
        await Context.SaveChangesAsync();

        var expected = await EmailSenderService.GetNextNewsletter(Context);
        Assert.IsNotNull(expected);
    }

    [TestMethod]
    public async Task GetNextNewsletter_WhenSendAfterIsInFuture_ReturnsNone()
    {
        var user = new User(string.Empty, true, false);
        Context.UserEmails.Add(new Data.Entities.Newsletter.UserEmail(user)
        {
            Body = string.Empty,
            Subject = string.Empty,
            Date = Today,
            SendAfter = DateTime.UtcNow.AddMinutes(1),
        });
        await Context.SaveChangesAsync();

        var expected = await EmailSenderService.GetNextNewsletter(Context);
        Assert.IsNull(expected);
    }

    [TestMethod]
    public async Task GetNextNewsletter_WhenDateIsTooFarGone_ReturnsNone()
    {
        var user = new User(string.Empty, true, false);
        Context.UserEmails.Add(new Data.Entities.Newsletter.UserEmail(user)
        {
            Body = string.Empty,
            Subject = string.Empty,
            Date = Today.AddDays(-2),
            SendAfter = DateTime.UtcNow.AddDays(-2),
        });
        await Context.SaveChangesAsync();

        var expected = await EmailSenderService.GetNextNewsletter(Context);
        Assert.IsNull(expected);
    }
}
