using FinerFettle.Web.Models.Exercise;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class EquipmentGroupViewModel
    {
        public EquipmentGroupViewModel(EquipmentGroup equipmentGroup, Models.User.User? user)
        {
            EquipmentGroup = equipmentGroup;
            User = user;
        }

        public EquipmentGroup EquipmentGroup { get; init; }
        public Models.User.User? User { get; init; }
    }
}
