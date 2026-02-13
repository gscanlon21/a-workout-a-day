using Web.Components.Users;
using Web.Views.Shared.Components.UnsupportedClient;

namespace Web.Test.Unit.Components;

[TestClass]
public class TestUnsupportedClientViewComponent
{
    [TestMethod]
    public async Task GetUnsupportedClientStatus_WhenUserIsGmail_ReturnsUnsupportedClientStatus()
    {
        var user = new Data.Entities.Users.User("test@gmail.com", acceptedTerms: true, isNewToFitness: false);
        var status = UnsupportedClientViewComponent.GetUnsupportedClient(user);
        Assert.AreEqual(UnsupportedClientViewModel.UnsupportedClient.Gmail, status);
    }

    [TestMethod]
    public async Task GetUnsupportedClientStatus_WhenUserIsFastmail_ReturnsSupportedClientStatus()
    {
        var user = new Data.Entities.Users.User("test@fastmail.com", acceptedTerms: true, isNewToFitness: false);
        var status = UnsupportedClientViewComponent.GetUnsupportedClient(user);
        Assert.AreEqual(UnsupportedClientViewModel.UnsupportedClient.None, status);
    }
}
