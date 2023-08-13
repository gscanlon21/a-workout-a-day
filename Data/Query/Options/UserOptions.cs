
using Core.Models.Newsletter;

namespace Data.Query.Options;

public class UserOptions : IOptions
{
    public bool NoUser { get; } = true;
    public int Id { get; }
    public int RefreshExercisesAfterXWeeks { get; }
    public IList<int> EquipmentIds { get; } = new List<int>();
    public bool IsNewToFitness { get; }
    public DateOnly CreatedDate { get; }

    public bool IgnoreProgressions { get; set; } = false;
    public bool IgnorePrerequisites { get; set; } = false;

    public UserOptions() { }

    public UserOptions(Entities.User.User user, Section? section)
    {
        NoUser = false;
        Id = user.Id;
        EquipmentIds = user.EquipmentIds.ToList();
        IsNewToFitness = user.IsNewToFitness;
        CreatedDate = user.CreatedDate;

        RefreshExercisesAfterXWeeks = section switch
        {
            Section.Functional => user.RefreshFunctionalEveryXWeeks,
            Section.Accessory => user.RefreshAccessoryEveryXWeeks,
            _ => 0
        };
    }
}
