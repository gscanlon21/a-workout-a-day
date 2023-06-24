using Lib.Dtos.Equipment;

namespace Lib.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for _Instruction.cshtml
/// </summary>
public class InstructionViewModel
{
    public InstructionViewModel() { }

    public InstructionViewModel(Instruction instruction, User.UserNewsletterViewModel? user)
    {
        Instruction = instruction;
        User = user;
    }

    public Instruction Instruction { get; init; } = null!;
    public User.UserNewsletterViewModel? User { get; init; }
}
