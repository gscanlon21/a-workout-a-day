using FinerFettle.Web.Models.Exercise;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class EquipmentViewModel
    {
        public EquipmentViewModel(IEnumerable<Equipment> allEquipment, IEnumerable<Equipment> userEquipment)
        {
            AllEquipment = allEquipment;
            UserEquipment = userEquipment;
        }

        public IEnumerable<Equipment> AllEquipment { get; init; }
        public IEnumerable<Equipment> UserEquipment { get; init; }
    }
}
