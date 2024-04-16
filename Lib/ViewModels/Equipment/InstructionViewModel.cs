using Lib.ViewModels.Exercise;
using Lib.ViewModels.User;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Lib.ViewModels.Equipment;

/// <summary>
/// Equipment that can be switched out for one another.
/// </summary>
[DebuggerDisplay("Name = {Name}")]
public class InstructionViewModel
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
    public ICollection<InstructionViewModel> Children { private get; init; } = [];

    public bool HasChildInstructions => Children.Any();

    /// <summary>
    /// Square when there's no equipment (likely rehab exercise) because it's similar to a checkbox, do all of them.    
    /// Disc when there is equipment (likely strengthening exercise), do one of them.
    /// 
    /// Should find a more explicit way to handle this eventually...
    /// </summary>
    public string ListStyleType => HasChildInstructions ? "disclosure-open"
        : (Order != null ? "decimal" : (Equipment == Core.Models.Equipment.Equipment.None ? "square" : "disc"));

    public IOrderedEnumerable<InstructionViewModel> GetChildInstructions(UserNewsletterViewModel? user)
    {
        return Children
            // Only show the optional equipment groups that the user owns equipment out of
            .Where(eg => user == null || (user.Equipment & eg.Equipment) != 0
                // Or the instruction doesn't have any equipment.
                || eg.Equipment == Core.Models.Equipment.Equipment.None)
            // Keep the order consistent across newsletters
            .OrderBy(eg => eg.Order ?? int.MaxValue)
            .ThenBy(eg => eg.Id);
    }

    [JsonInclude]
    public InstructionViewModel? Parent { get; init; } = null!;

    [JsonInclude]
    public VariationViewModel Variation { get; init; } = null!;
}
