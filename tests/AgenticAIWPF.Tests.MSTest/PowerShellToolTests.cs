// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Tests.MSTest
// File:         PowerShellToolTests.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



using AgentAILib.ToolFunctions.OSTools;




namespace AgenticAIWPF.Tests.MSTest;





[TestClass]
public class PowerShellToolTests
{

    private readonly PowerShellTool _tool = new();








    [TestMethod]
    public async Task RunReadOnly_WithBlockedVerb_ReturnsFail()
    {
        var result = await _tool.RunReadOnly("Remove-Item 'C:\\temp\\test.txt'");

        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.FailureReason, "Remove");
    }








    [TestMethod]
    public async Task RunReadOnly_WithEmptyCommand_ReturnsFail()
    {
        var result = await _tool.RunReadOnly(string.Empty);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.FailureReason);
    }








    [TestMethod]
    public async Task RunReadOnly_WithTimeoutAboveMaximum_ReturnsFail()
    {
        var result = await _tool.RunReadOnly("Get-Process", 31);

        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.FailureReason, "Timeout");
    }








    [TestMethod]
    public async Task RunReadOnly_WithTimeoutBelowMinimum_ReturnsFail()
    {
        var result = await _tool.RunReadOnly("Get-Process", 0);

        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.FailureReason, "Timeout");
    }
}