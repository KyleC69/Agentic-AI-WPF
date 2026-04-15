// Build Date: 2026/04/13
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Tests.MSTest
// File:         SafeCommandRunnerTests.cs
// Author: GitHub Copilot
// Build Num: 204200



using AgentAILib.ToolFunctions;




namespace AgenticAIWPF.Tests.MSTest;





[TestClass]
public class SafeCommandRunnerTests
{
    [TestMethod]
    public async Task ExecuteAsyncWithCmdEchoReturnsSuccessAndOutput()
    {
        CommandExecutor executor = new();

        var result = await executor.ExecuteAsync("cmd.exe", "/c echo hello world");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.ExitCode);
        Assert.AreEqual("hello world", result.Output);
    }





    [TestMethod]
    public async Task ExecuteAsyncWithMissingCommandReturnsNotFound()
    {
        CommandExecutor executor = new();

        var result = await executor.ExecuteAsync($"missing-command-{Guid.NewGuid():N}.exe");

        Assert.IsFalse(result.Success);
        Assert.AreEqual(-1, result.ExitCode);
        StringAssert.StartsWith(result.Error, "Command not found: '");
    }





    [TestMethod]
    public void FailureFactoryReturnsExpectedFailureShape()
    {
        var result = CommandResult.Failure("boom");

        Assert.IsFalse(result.Success);
        Assert.AreEqual(-1, result.ExitCode);
        Assert.AreEqual("boom", result.Error);
        Assert.AreEqual(string.Empty, result.Output);
    }





    [TestMethod]
    public void NotFoundFactoryReturnsExpectedFailureShape()
    {
        var result = CommandResult.NotFound("missing.exe");

        Assert.IsFalse(result.Success);
        Assert.AreEqual(-1, result.ExitCode);
        Assert.AreEqual(string.Empty, result.Output);
        StringAssert.Contains(result.Error, "missing.exe");
    }





    [TestMethod]
    public void TimeoutFactoryReturnsTimedOutResult()
    {
        var result = CommandResult.Timeout(250, "partial");

        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.TimedOut);
        Assert.AreEqual(-1, result.ExitCode);
        Assert.AreEqual("partial", result.Output);
        Assert.AreEqual("Command timed out after 250ms", result.Error);
    }
}