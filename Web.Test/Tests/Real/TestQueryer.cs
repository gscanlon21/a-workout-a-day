using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Data.QueryBuilder;
using Web.Entities.User;
using Web.Models.Exercise;
using Web.Models.Newsletter;
using Web.Models.User;
using Web.ViewModels.Newsletter;

namespace Web.Test.Tests.Real;


[TestClass]
public class TestQueryer : RealDatabase
{
    [TestMethod]
    public async Task ExerciseQueryer_WithExerciseType_ReturnsCorrectExerciseType()
    {
        var results = await new ExerciseQueryBuilder(Context)
            .WithExerciseType(ExerciseType.Main)
            .Build()
            .Query();

        Assert.IsTrue(results.All(vm => vm.ExerciseVariation.ExerciseType.HasFlag(ExerciseType.Main)));
    }
}