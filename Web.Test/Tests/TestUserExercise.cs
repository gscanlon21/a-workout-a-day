using Web.Entities.User;

namespace Web.Test.Tests;

[TestClass]
public class TestUserExercise : Database
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