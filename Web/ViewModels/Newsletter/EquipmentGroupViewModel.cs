using Web.Entities.Equipment;

namespace Web.ViewModels.Newsletter;

public class EquipmentGroupViewModel
{
    public EquipmentGroupViewModel(EquipmentGroup equipmentGroup, User.UserNewsletterViewModel? user)
    {
        EquipmentGroup = equipmentGroup;
        User = user;
    }

    public EquipmentGroup EquipmentGroup { get; }
    public User.UserNewsletterViewModel? User { get; }
}
