using Data.Entities.Equipment;
using Data.Models.User;

namespace Data.Models.Newsletter;

/// <summary>
/// Viewmodel for _Instruction.cshtml
/// </summary>
public class InstructionModel
{
    public InstructionModel(Instruction instruction, UserNewsletterModel? user)
    {
        Instruction = instruction;
        User = user;
    }

    public Instruction Instruction { get; }
    public UserNewsletterModel? User { get; }
}
