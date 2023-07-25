using Lib.ViewModels.Equipment;
using System.Text.Json.Serialization;

namespace Lib.ViewModels.User;

/// <summary>
/// Maps a user with their equipment.
/// </summary>
public class UserEquipmentViewModel
{
    public int EquipmentId { get; init; }

    public int UserId { get; init; }

    [JsonInclude]
    public UserViewModel User { get; init; } = null!;

    [JsonInclude]
    public EquipmentViewModel Equipment { get; init; } = null!;
}
