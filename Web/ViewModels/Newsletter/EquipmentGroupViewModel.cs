using Web.Entities.Equipment;

namespace Web.ViewModels.Newsletter;

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
