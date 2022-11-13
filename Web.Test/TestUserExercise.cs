using Web.Entities.User;

namespace Web.Test;

[TestClass]
public class TestUserExercise
{
    /// <summary>
    /// Certain functionality relies on the default progression also being the MinUserProgression.
    /// </summary>
    [TestMethod]
    public void UserExercise_Default_HasProgressionEqualMinProgression()
    {
        var userExercise = new UserExercise();

        Assert.AreEqual(userExercise.Progression, UserExercise.MinUserProgression);
    }
}