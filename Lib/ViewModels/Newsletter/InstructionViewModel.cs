
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
        // Disabling the friendly equipment exercise name and moving that over to a title attribute.
        if (true || string.IsNullOrWhiteSpace(Instruction.Name))
        {
            if (User == null)
            {
                return string.Join(" | ", Instruction.Equipment.Select(e => e.Name));
            }
            else
            {
                return string.Join(" | ", Instruction.Equipment.IntersectBy(User.EquipmentIds, e => e.Id).Select(e => e.Name));
            }
        }

        // This is the friendly equipment exercise name.
        return Instruction.Name;
    }
}
