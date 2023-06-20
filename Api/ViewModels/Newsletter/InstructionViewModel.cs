using Data.Entities.Equipment;

namespace Api.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for _Instruction.cshtml
/// </summary>
public class InstructionViewModel
{
    public InstructionViewModel(Instruction instruction, User.UserNewsletterViewModel? user)
    {
        Instruction = instruction;
        User = user;
    }

    public Instruction Instruction { get; }
    public User.UserNewsletterViewModel? User { get; }
}
