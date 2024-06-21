using Core.Dtos.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Core.Dtos.Exercise;

/// <summary>
/// Equipment that can be switched out for one another.
/// </summary>
[Table("instruction")]
[DebuggerDisplay("Name = {Name}")]
public class InstructionDto
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int? Order { get; init; }

    /// <summary>
    /// Notes about the variation (not externally shown)
    /// </summary>
    [JsonIgnore]
    public string? Notes { get; init; } = null;

    /// <summary>
    /// Friendly name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// A link to show the user how to complete the exercise w/ this equipment.
    /// </summary>
    public string? Link { get; init; }

    public Models.Equipment.Equipment Equipment { get; init; }

    public string? DisabledReason { get; init; } = null;

    public virtual ICollection<InstructionDto> Children { get; init; } = [];

    [JsonIgnore]
    public virtual InstructionDto? Parent { get; init; } = null!;

    [JsonIgnore]
    public virtual VariationDto Variation { get; init; } = null!;

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

    public IOrderedEnumerable<InstructionDto> GetChildInstructions(UserNewsletterDto? user)
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
}
