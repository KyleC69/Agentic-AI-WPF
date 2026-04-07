// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         SafeCommandRunnerTests.cs
// Author: Kyle L. Crowder
// Build Num: 213001



using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class SafeCommandRunnerTests
{
    private string _sandboxRoot = string.Empty;








    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_sandboxRoot))
        {
            Directory.Delete(_sandboxRoot, true);
        }
    }








    [TestInitialize]
    public void Initialize()
    {
        _sandboxRoot = Path.Combine(Path.GetTempPath(), "safe-command-tests", Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(_sandboxRoot);
        File.WriteAllText(Path.Combine(_sandboxRoot, "sample.txt"), "sample content");
    }








    [TestMethod]
    public void RunCatExistingFileReturnsContents()
    {
        SafeCommandRunner runner = new(_sandboxRoot);

        var result = runner.Run("cat sample.txt");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("sample content", result.Value);
    }








    [TestMethod]
    public void RunCatMissingFileReturnsFailure()
    {
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxRoot);

        var result = runner.Run("cat missing.txt");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("File not found.", result.Error);
    }








    [TestMethod]
    public void RunCatOutsideSandboxReturnsAccessDenied()
    {
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxRoot);

        var result = runner.Run("cat ..\\outside.txt");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Access denied.", result.Error);
    }








    [TestMethod]
    public void RunDirReturnsSandboxFileNames()
    {
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxRoot);

        var result = runner.Run("dir");

        Assert.IsTrue(result.Success);
        StringAssert.Contains(result.Value, "sample.txt");
    }








    [TestMethod]
    public void RunEchoReturnsProvidedText()
    {
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxRoot);

        var result = runner.Run("echo hello world");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("hello world", result.Value);
    }








    [TestMethod]
    public void RunWithDisallowedCommandReturnsFailure()
    {
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxRoot);

        var result = runner.Run("del sample.txt");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Command 'del' is not allowed.", result.Error);
    }








    [TestMethod]
    public void RunWithEmptyInputReturnsFailure()
    {
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxRoot);

        var result = runner.Run("   ");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("No command provided.", result.Error);
    }
}