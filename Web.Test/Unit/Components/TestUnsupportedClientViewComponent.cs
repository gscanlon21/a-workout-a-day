using Web.Components.User;
using Web.ViewModels.Components.User;

namespace Web.Test.Unit.Components;

[TestClass]
public class TestUnsupportedClientViewComponent
{
    [TestMethod]
    public async Task GetUnsupportedClientStatus_WhenUserIsGmail_ReturnsUnsupportedClientStatus()
    {
        var user = new Data.Entities.User.User("test@gmail.com", acceptedTerms: true, isNewToFitness: false);
        var status = UnsupportedClientViewComponent.GetUnsupportedClient(user);
        Assert.AreEqual(status, UnsupportedClientViewModel.UnsupportedClient.Gmail);
    }

    [TestMethod]
    public async Task GetUnsupportedClientStatus_WhenUserIsFastmail_ReturnsSupportedClientStatus()
    {
        var user = new Data.Entities.User.User("test@fastmail.com", acceptedTerms: true, isNewToFitness: false);
        var status = UnsupportedClientViewComponent.GetUnsupportedClient(user);
        Assert.AreEqual(status, UnsupportedClientViewModel.UnsupportedClient.None);
    }
}
