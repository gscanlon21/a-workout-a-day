namespace Data.Query.Options;

public class ProficiencyOptions : IOptions
{
    /// <summary>
    ///     If true and the User's exercise proficiency is above the exercise's proficiency:
    ///     ... Will choose exercise that fall at or under the exercise's proficiency level.
    ///     Otherwise, will choose variations that fall within the User's exiercise progression range. 
    /// </summary>
    public bool DoCapAtProficiency { get; set; } = false;
}