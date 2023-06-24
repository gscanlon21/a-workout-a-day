using Lib.ViewModels.Equipment;

namespace Lib.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for _Instruction.cshtml
/// </summary>
public class InstructionViewModel
{
    public InstructionViewModel() { }

    public InstructionViewModel(Equipment.InstructionViewModel instruction, User.UserNewsletterViewModel? user)
    {
        Instruction = instruction;
        User = user;
    }

    public Equipment.InstructionViewModel Instruction { get; init; } = null!;
    public User.UserNewsletterViewModel? User { get; init; }
}
