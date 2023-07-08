
namespace Data.Data.Query.Options;

public class UserOptions : IOptions
{
    public bool NoUser { get; } = true;
    public int Id { get; }
    public IList<int> EquipmentIds { get; } = new List<int>();
    public bool IsNewToFitness { get; }
    public DateOnly CreatedDate { get; }

    public bool IgnoreProgressions { get; set; } = false;
    public bool IgnorePrerequisites { get; set; } = false;

    public UserOptions() { }

    public UserOptions(Entities.User.User user)
    {
        NoUser = false;
        Id = user.Id;
        EquipmentIds = user.EquipmentIds.ToList();
        IsNewToFitness = user.IsNewToFitness;
        CreatedDate = user.CreatedDate;
    }
}
