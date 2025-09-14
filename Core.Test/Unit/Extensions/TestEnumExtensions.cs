﻿using Core.Code.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Core.Test.Unit.Extensions;

[TestClass]
public class TestEnumViewExtensions
{
    private enum TestEnum
    {
        A = 0,
        B = 1,
        C = 2,
        D = 3,
        E = 4,
    }

    [Flags]
    private enum TestEnumFlags
    {
        [Display(Name = "A")]
        A = 0,
        [Display(Name = "B")]
        B = 1 << 0,
        [Display(Name = "C")]
        C = 1 << 1,
        [Display(Name = "D")]
        D = 1 << 2,
        [Display(Name = "E")]
        E = 1 << 3,
        [Display(Name = "BE")]
        BE = B | E
    }

    [TestMethod]
    public async Task GetSingleValues64_ReturnsSingleValues()
    {
        var expected = new TestEnumFlags[] { TestEnumFlags.B, TestEnumFlags.C, TestEnumFlags.D, TestEnumFlags.E };
        Assert.IsTrue(expected.SequenceEqual(EnumExtensions.GetSingleValues<TestEnumFlags>().OrderBy(e => e)));
    }

    [TestMethod]
    public async Task GetDisplayValues_ReturnsExpected()
    {
        var expected = new TestEnumFlags[] { TestEnumFlags.A, TestEnumFlags.B, TestEnumFlags.C, TestEnumFlags.D, TestEnumFlags.E, TestEnumFlags.BE };
        Assert.IsTrue(expected.SequenceEqual(EnumExtensions.GetDisplayValues<TestEnumFlags>().OrderBy(e => e)));
    }

    [TestMethod]
    public async Task HasAnyFlag_ReturnsExpected()
    {
        Assert.IsTrue((TestEnumFlags.A | TestEnumFlags.B).HasAnyFlag(TestEnumFlags.BE));
    }

    [TestMethod]
    public async Task As_ReturnsExpected()
    {
        Assert.AreEqual(
            TestEnumFlags.A,
            TestEnum.A.As<TestEnumFlags>()
        );
    }

    [TestMethod]
    public async Task As_ReturnsExpected2()
    {
        Assert.AreEqual(
            TestEnum.A,
            TestEnumFlags.BE.As<TestEnum>(defaultVal: TestEnum.A)
        );
    }

    [TestMethod]
    public async Task UnsetFlag_ReturnsExpected()
    {
        Assert.AreEqual(
            TestEnumFlags.B,
            TestEnumFlags.BE.UnsetFlag(TestEnumFlags.E)
        );
    }

    [TestMethod]
    public async Task GetSingleOrNoneValues_ReturnsExpected()
    {
        var expected = new TestEnumFlags[] { TestEnumFlags.A, TestEnumFlags.B, TestEnumFlags.C, TestEnumFlags.D, TestEnumFlags.E };
        Assert.IsTrue(expected.SequenceEqual(EnumExtensions.GetSingleOrNoneValues<TestEnumFlags>().OrderBy(e => e)));
    }

    [TestMethod]
    public async Task GetSubValues_ReturnsExpected()
    {
        var expected = new TestEnumFlags[] { TestEnumFlags.B, TestEnumFlags.E };
        Assert.IsTrue(expected.SequenceEqual(EnumExtensions.GetSubValues(TestEnumFlags.BE).OrderBy(e => e)));
    }

    [TestMethod]
    public async Task GetMultiValues_ReturnsExpected()
    {
        var expected = new TestEnumFlags[] { TestEnumFlags.BE };
        Assert.IsTrue(expected.SequenceEqual(EnumExtensions.GetMultiValues<TestEnumFlags>().OrderBy(e => e)));
    }

    [TestMethod]
    public async Task GetNotNoneValues32_ReturnsExpected()
    {
        var expected = new TestEnumFlags[] { TestEnumFlags.B, TestEnumFlags.C, TestEnumFlags.D, TestEnumFlags.E, TestEnumFlags.BE };
        Assert.IsTrue(expected.SequenceEqual(EnumExtensions.GetNotNoneValues<TestEnumFlags>().OrderBy(e => e)));
    }

    [TestMethod]
    public async Task GetValuesExcluding_ReturnsExpected()
    {
        var expected = new TestEnumFlags[] { TestEnumFlags.A, TestEnumFlags.C, TestEnumFlags.D, TestEnumFlags.E, TestEnumFlags.BE };
        Assert.IsTrue(expected.SequenceEqual(EnumExtensions.GetValuesExcluding(TestEnumFlags.B).OrderBy(e => e)));
    }

    [TestMethod]
    public async Task GetSingleValuesExcludingAny_ReturnsExpected()
    {
        var expected = new TestEnumFlags[] { TestEnumFlags.C, TestEnumFlags.D };
        Assert.IsTrue(expected.SequenceEqual(EnumExtensions.GetSingleValues(excludingAny: TestEnumFlags.BE).OrderBy(e => e)));
    }
}
