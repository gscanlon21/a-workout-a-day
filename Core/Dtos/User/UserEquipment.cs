

using Core.Dtos.Equipment;

namespace Core.Dtos.User;

/// <summary>
/// Maps a user with their equipment.
/// </summary>
public interface IUserEquipment
{
    public int EquipmentId { get; init; }

    public int UserId { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.User.User.UserEquipments))]
    public IUser User { get; init; }

    //[JsonIgnore, InverseProperty(nameof(EquipmentDto.UserEquipments))]
    public IEquipment Equipment { get; init; }
}
