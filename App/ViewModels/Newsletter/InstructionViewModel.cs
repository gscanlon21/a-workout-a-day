using App.Dtos.Equipment;

namespace App.ViewModels.Newsletter;

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
