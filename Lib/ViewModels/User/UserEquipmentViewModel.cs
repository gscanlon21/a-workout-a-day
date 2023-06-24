using Lib.ViewModels.Equipment;

namespace Lib.ViewModels.User;

/// <summary>
/// Maps a user with their equipment.
/// </summary>
public class UserEquipmentViewModel
{
    public int EquipmentId { get; init; }

    public int UserId { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.User.User.UserEquipments))]
    public virtual UserViewModel User { get; init; } = null!;

    //[JsonIgnore, InverseProperty(nameof(EquipmentDto.UserEquipments))]
    public virtual EquipmentViewModel Equipment { get; init; } = null!;
}
