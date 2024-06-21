using Core.Code.Extensions;
using Lib.Pages.Newsletter;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Lib.Pages.Shared.Exercise;


public class InstructionViewModel(Exercise.InstructionViewModel2 instruction, UserNewsletterViewModel? user)
{
    public Exercise.InstructionViewModel2 Instruction { get; } = instruction;
    public UserNewsletterViewModel? User { get; } = user;

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


/// <summary>
/// Equipment that can be switched out for one another.
/// </summary>
[DebuggerDisplay("Name = {Name}")]
public class InstructionViewModel2
{
    public int Id { get; init; }

    public int? Order { get; init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// A link to show the user how to complete the exercise w/ this equipment.
    /// </summary>
    public string? Link { get; init; }

    public string? DisabledReason { get; init; } = null;

    public Core.Models.Equipment.Equipment Equipment { get; init; }

    [JsonInclude]
    public ICollection<InstructionViewModel2> Children { private get; init; } = [];

    public bool HasChildInstructions => Children.Any();

    /// <summary>
    /// Square when there's no equipment (likely rehab exercise) because it's similar to a checkbox, do all of them.    
    /// Disc when there is equipment (likely strengthening exercise), do one of them.
    /// 
    /// Should find a more explicit way to handle this eventually...
    /// </summary>
    public string ListStyleType
    {
        get
        {
            if (Order.HasValue && Order.Value >= 0 && Order.Value < 100)
            {
                // https://en.wikipedia.org/wiki/Whitespace_character#Spaces_in_Unicode
                return $"'{Order}.\\2004'";
            }
            else if (HasChildInstructions)
            {
                return "disclosure-open";
            }
            else if (Equipment != Core.Models.Equipment.Equipment.None)
            {
                return "disc";
            }

            return "square";
        }
    }

    public IOrderedEnumerable<InstructionViewModel2> GetChildInstructions(UserNewsletterViewModel? user)
    {
        return Children
            // Only show the optional equipment groups that the user owns equipment out of
            .Where(eg => user == null || (user.Equipment & eg.Equipment) != 0
                // Or the instruction doesn't have any equipment.
                || eg.Equipment == Core.Models.Equipment.Equipment.None)
            // Keep the order consistent across newsletters
            .OrderByDescending(eg => eg.HasChildInstructions && !eg.Order.HasValue)
            .ThenBy(eg => eg.Order ?? int.MaxValue)
            .ThenBy(eg => eg.Name)
            .ThenBy(eg => eg.Id);
    }

    [JsonInclude]
    public InstructionViewModel? Parent { get; init; } = null!;

    [JsonInclude]
    public VariationViewModel Variation { get; init; } = null!;
}
