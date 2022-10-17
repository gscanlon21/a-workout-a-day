using FinerFettle.Web.Models.Exercise;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class EquipmentGroupViewModel
    {
        public EquipmentGroupViewModel(EquipmentGroup equipmentGroup, User.UserNewsletterViewModel? user)
        {
            EquipmentGroup = equipmentGroup;
            User = user;
        }

        public EquipmentGroup EquipmentGroup { get; init; }
        public User.UserNewsletterViewModel? User { get; init; }
    }
}
