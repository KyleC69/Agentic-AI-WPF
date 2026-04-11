// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         SandboxFileWriterTests.cs
// Author: Kyle L. Crowder
// Build Num: 213002



using DataIngestionLib.ToolFunctions.FileSystemWriters;

namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class SandboxFileWriterTests
{
    private string _sandboxRoot = string.Empty;
    private FileSystemWriterTool _tool = null!;








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
        _sandboxRoot = Path.Combine(Path.GetTempPath(), "writer-tool-tests", Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(_sandboxRoot);
        _tool = new FileSystemWriterTool([_sandboxRoot]);
    }









    [TestMethod]
    public void WriteTextWithAllowedPathWritesFile()
    {
        var result = _tool.WriteText("sample.txt", "hello writer");

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(Path.Combine(_sandboxRoot, "sample.txt"), result.Value.FullPath);
        Assert.AreEqual(_sandboxRoot, result.Value.AllowedRoot);
        Assert.AreEqual(12, result.Value.CharacterCount);
        Assert.AreEqual("hello writer", File.ReadAllText(Path.Combine(_sandboxRoot, "sample.txt")));
    }









    [TestMethod]
    public void WriteTextOutsideAllowedRootReturnsFailure()
    {
        var result = _tool.WriteText("..\\blocked.txt", "blocked");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Access denied: '..\\blocked.txt' is outside the configured allowlisted roots.", result.Error);
    }









    [TestMethod]
    public void WriteTextWithEmptyPathReturnsFailure()
    {
        var result = _tool.WriteText("  ", "content");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Path cannot be empty.", result.Error);
    }
}