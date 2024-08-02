using Core.Dtos.Exercise;
using Core.Dtos.User;

namespace Lib.Pages.Shared.Exercise;


public class InstructionViewModel(InstructionDto instruction, UserNewsletterDto? user)
{
    public InstructionDto Instruction { get; } = instruction;
    public UserNewsletterDto? User { get; } = user;

    /// <summary>
    /// Gets the friendly instruction display name.
    /// </summary>
    public string GetDisplayName()
    {
        var equipmentDisplayName = GetEquipmentDisplayName();

        if (string.IsNullOrWhiteSpace(equipmentDisplayName))
        {
            // Don't throw an exception if null, it's not a fatal error.
            return Instruction.Name ?? Instruction.Link ?? "ERROR!";
        }
        else if (string.IsNullOrWhiteSpace(Instruction.Name))
        {
            return equipmentDisplayName;
        }

        return $"{Instruction.Name} ({equipmentDisplayName})";
    }

    /// <summary>
    /// Gets the friendly equipment display name.
    /// </summary>
    private string? GetEquipmentDisplayName()
    {
        var equipments = EnumExtensions.GetSingleValues32<Core.Models.Equipment.Equipment>().Where(e => Instruction.Equipment.HasFlag(e)).ToList();
        if (equipments.Count == 0)
        {
            return null;
        }

        if (User == null)
        {
            return string.Join(" | ", equipments.Select(e => e.GetDisplayName32()));
        }
        else
        {
            return string.Join(" | ", equipments.Where(e => User.Equipment.HasFlag(e)).Select(e => e.GetDisplayName32()));
        }
    }
}
