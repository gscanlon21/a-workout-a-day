using Core.Models.Equipment;
using Core.Models.User;
using Web.Components.Users;
using Web.Views.Shared.Components.Equipment;

namespace Web.Test.Unit.Components;

[TestClass]
public class TestEquipmentViewComponent
{
    [TestMethod]
    public async Task GetUserEquipmentStatus_WhenUserHasNoEquipment_ReturnsNoEquipmentStatus()
    {
        var status = EquipmentViewComponent.GetUserEquipmentStatus(new Data.Entities.Users.User(string.Empty, acceptedTerms: true, isNewToFitness: false)
        {
            Equipment = Equipment.None
        });

        Assert.AreEqual(EquipmentViewModel.UserEquipmentStatus.MissingEquipment, status);
    }

    [TestMethod]
    public async Task GetUserEquipmentStatus_WhenUserHasSomeEquipment_ReturnsPartialEquipmentStatus()
    {
        var user = new Data.Entities.Users.User(string.Empty, acceptedTerms: true, isNewToFitness: false)
        {
            Equipment = Equipment.FlatBench
        };

        var status = EquipmentViewComponent.GetUserEquipmentStatus(user);
        Assert.AreEqual(EquipmentViewModel.UserEquipmentStatus.MissingResistanceEquipment, status);
    }

    [TestMethod]
    public async Task GetUserEquipmentStatus_WhenUserHasEquipment_ReturnsEquipmentStatus()
    {
        var user = new Data.Entities.Users.User(string.Empty, acceptedTerms: true, isNewToFitness: false)
        {
            Equipment = Equipment.GymnasticRings
        };

        var status = EquipmentViewComponent.GetUserEquipmentStatus(user);
        Assert.AreEqual(EquipmentViewModel.UserEquipmentStatus.None, status);
    }

    [TestMethod]
    public async Task GetUserEquipmentStatus_WhenUserHasNoEquipmentAndIsOnlyMobility_ReturnsNoEquipmentStatus()
    {
        var user = new Data.Entities.Users.User(string.Empty, acceptedTerms: true, isNewToFitness: false)
        {
            SendDays = Days.None,
            IncludeMobilityWorkouts = true
        };

        var status = EquipmentViewComponent.GetUserEquipmentStatus(user);
        Assert.AreEqual(EquipmentViewModel.UserEquipmentStatus.None, status);
    }
}
