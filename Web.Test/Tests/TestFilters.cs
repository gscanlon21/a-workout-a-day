using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Data.QueryBuilder;
using Web.Models.Exercise;
using Web.Models.Newsletter;
using Web.Models.User;

namespace Web.Test.Tests;


[TestClass]
public class TestFilters : Database
{
    [TestMethod]
    public void Filters_SportsFocus()
    {
        //var groups = Filters.FilterSportsFocus
        Assert.IsTrue(true);
    }
}