// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: GitHub Copilot
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}



using AgentAILib.ToolFunctions.OSTools;




namespace AgenticAIWPF.Tests.MSTest;




[TestClass]
public class PowerShellToolTests
{

    private readonly PowerShellTool _tool = new();




    [TestMethod]
    public void GetBlockReason_WithEmptyCommand_IsHandledByRunReadOnly()
    {
        // GetBlockReason itself is called after the empty-string guard in RunReadOnly;
        // verify a whitespace-only string is not null/empty for coverage purposes.
        var reason = _tool.GetBlockReason("   ");

        // No cmdlets in whitespace — no block reason from verb analysis.
        Assert.IsNull(reason);
    }




    [TestMethod]
    public void GetBlockReason_WithDestructiveVerb_ReturnsBlockedReason()
    {
        var reason = _tool.GetBlockReason("Remove-Item 'C:\\temp\\test.txt'");

        Assert.IsNotNull(reason);
        StringAssert.Contains(reason, "Remove");
    }




    [TestMethod]
    public void GetBlockReason_WithInvokeExpressionVerb_ReturnsBlockedReason()
    {
        var reason = _tool.GetBlockReason("Invoke-Expression 'whoami'");

        Assert.IsNotNull(reason);
        StringAssert.Contains(reason, "Invoke");
    }




    [TestMethod]
    public void GetBlockReason_WithStopProcessVerb_ReturnsBlockedReason()
    {
        var reason = _tool.GetBlockReason("Get-Process chrome | Stop-Process");

        Assert.IsNotNull(reason);
        StringAssert.Contains(reason, "Stop");
    }




    [TestMethod]
    public void GetBlockReason_WithBlockedAlias_iex_ReturnsBlockedReason()
    {
        var reason = _tool.GetBlockReason("iex 'whoami'");

        Assert.IsNotNull(reason);
        StringAssert.Contains(reason, "iex");
    }




    [TestMethod]
    public void GetBlockReason_WithBlockedAlias_rm_ReturnsBlockedReason()
    {
        var reason = _tool.GetBlockReason("rm C:\\temp\\file.txt");

        Assert.IsNotNull(reason);
        StringAssert.Contains(reason, "rm");
    }




    [TestMethod]
    public void GetBlockReason_WithCallOperator_ReturnsBlockedReason()
    {
        var reason = _tool.GetBlockReason("& 'C:\\Windows\\System32\\cmd.exe'");

        Assert.IsNotNull(reason);
        StringAssert.Contains(reason, "'&'");
    }




    [TestMethod]
    public void GetBlockReason_WithDotSourcing_ReturnsBlockedReason()
    {
        var reason = _tool.GetBlockReason(". C:\\scripts\\deploy.ps1");

        Assert.IsNotNull(reason);
        StringAssert.Contains(reason, "Dot-sourcing");
    }




    [TestMethod]
    public void GetBlockReason_WithBlockedCommandName_FormatVolume_ReturnsBlockedReason()
    {
        var reason = _tool.GetBlockReason("Format-Volume -DriveLetter C -FileSystem NTFS");

        Assert.IsNotNull(reason);
        StringAssert.Contains(reason, "Format-Volume");
    }




    [TestMethod]
    public void GetBlockReason_WithBlockedCommandName_OutFile_ReturnsBlockedReason()
    {
        var reason = _tool.GetBlockReason("Get-Process | Out-File -FilePath C:\\output.txt");

        Assert.IsNotNull(reason);
        StringAssert.Contains(reason, "Out-File");
    }




    [TestMethod]
    public void GetBlockReason_WithAllowedGetVerb_ReturnsNull()
    {
        var reason = _tool.GetBlockReason("Get-Process");

        Assert.IsNull(reason);
    }




    [TestMethod]
    public void GetBlockReason_WithAllowedPipeline_ReturnsNull()
    {
        var reason = _tool.GetBlockReason(
            "Get-Process | Sort-Object CPU -Descending | Select-Object -First 10 | Format-Table Name,CPU");

        Assert.IsNull(reason);
    }




    [TestMethod]
    public void GetBlockReason_WithAllowedConvertToJson_ReturnsNull()
    {
        var reason = _tool.GetBlockReason("Get-Service | ConvertTo-Json -Depth 2");

        Assert.IsNull(reason);
    }




    [TestMethod]
    public void GetBlockReason_WithSetVerbInPipeline_BlocksEntirePipeline()
    {
        // A read command piped into a write command must still be blocked.
        var reason = _tool.GetBlockReason("Get-Content file.txt | Set-Content output.txt");

        Assert.IsNotNull(reason);
        StringAssert.Contains(reason, "Set");
    }




    [TestMethod]
    public async Task RunReadOnly_WithEmptyCommand_ReturnsFail()
    {
        var result = await _tool.RunReadOnly(string.Empty);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.FailureReason);
    }




    [TestMethod]
    public async Task RunReadOnly_WithTimeoutBelowMinimum_ReturnsFail()
    {
        var result = await _tool.RunReadOnly("Get-Process", timeoutSeconds: 0);

        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.FailureReason, "Timeout");
    }




    [TestMethod]
    public async Task RunReadOnly_WithTimeoutAboveMaximum_ReturnsFail()
    {
        var result = await _tool.RunReadOnly("Get-Process", timeoutSeconds: 31);

        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.FailureReason, "Timeout");
    }




    [TestMethod]
    public async Task RunReadOnly_WithBlockedVerb_ReturnsFail()
    {
        var result = await _tool.RunReadOnly("Remove-Item 'C:\\temp\\test.txt'");

        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.FailureReason, "Remove");
    }




    [TestMethod]
    public void AllowedVerbs_ContainsExpectedReadOnlyVerbs()
    {
        string[] required = ["Get", "Select", "Where", "Format", "Measure", "Test", "Sort", "Group"];

        foreach (var verb in required)
        {
            Assert.IsTrue(
                PowerShellTool.AllowedVerbs.Contains(verb),
                $"AllowedVerbs should contain '{verb}'.");
        }
    }




    [TestMethod]
    public void BlockedTokens_ContainsKnownDangerousAliases()
    {
        string[] required = ["iex", "rm", "del", "kill", "ni", "sc"];

        foreach (var alias in required)
        {
            Assert.IsTrue(
                PowerShellTool.BlockedTokens.Contains(alias),
                $"BlockedTokens should contain '{alias}'.");
        }
    }

}
