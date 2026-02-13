using System.Runtime.CompilerServices;

namespace Data.Test.Code.Attributes;

internal class TestMethodOnRemoteAttribute : TestMethodAttribute
{
    public TestMethodOnRemoteAttribute([CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1) : base(callerFilePath, callerLineNumber) { }

    public override async Task<TestResult[]> ExecuteAsync(ITestMethod testMethod)
    {
        if (DebugConsts.IsDebug)
        {
            return
            [
                new TestResult
                {
                    Outcome = UnitTestOutcome.Inconclusive,
                    TestContextMessages = "This test can only run on remote."
                }
            ];
        }

        return await base.ExecuteAsync(testMethod);
    }
}
