using Core.Code.Extensions;

namespace Lib.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for _Instruction.cshtml
/// </summary>
public class InstructionViewModel
{
    public InstructionViewModel(Equipment.InstructionViewModel instruction, User.UserNewsletterViewModel? user)
    {
        Instruction = instruction;
        User = user;
    }

    public Equipment.InstructionViewModel Instruction { get; } = null!;
    public User.UserNewsletterViewModel? User { get; }

    public string GetDisplayName()
    {
        var vals = EnumExtensions.GetSingleValues32<Core.Models.Equipment.Equipment>().Where(e => Instruction.Equipment.HasFlag(e));
        // Disabling the friendly equipment exercise name and moving that over to a title attribute.
        if (true || string.IsNullOrWhiteSpace(Instruction.Name))
        {
            if (User == null)
            {
                return string.Join(" | ", vals.Select(e => e.GetDisplayName32()));
            }
            else
            {
                return string.Join(" | ", vals.Where(e => User.Equipment.HasFlag(e)).Select(e => e.GetDisplayName32()));
            }
        }

        // This is the friendly equipment exercise name.
        return Instruction.Name;
    }
}
