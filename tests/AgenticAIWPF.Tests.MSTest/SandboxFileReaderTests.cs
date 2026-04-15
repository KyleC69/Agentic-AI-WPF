// Build Date: 2026/04/10
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Tests.MSTest
// File:         SandboxFileReaderTests.cs
// Author: GitHub Copilot

using AgentAILib.ToolFunctions.FileSystemReaders;

namespace AgenticAIWPF.Tests.MSTest;

[TestClass]
public class SandboxFileReaderTests
{
    private string _sandboxRoot = string.Empty;
    private FileContentsReadingTool _tool = null!;

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
        _sandboxRoot = Path.Combine(Path.GetTempPath(), "reader-tool-tests", Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(_sandboxRoot);
        _tool = new FileContentsReadingTool([_sandboxRoot]);
    }

    [TestMethod]
    public void ReadFileExistingFileReturnsContent()
    {
        var filePath = Path.Combine(_sandboxRoot, "sample.txt");
        File.WriteAllText(filePath, "hello reader");

        var result = _tool.ReadFileContents("sample.txt");

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual("hello reader", result.Value.Content);
        Assert.AreEqual(filePath, result.Value.FullPath);
        Assert.AreEqual(_sandboxRoot, result.Value.AllowedRoot);
        Assert.IsNull(result.Error);
    }

    [TestMethod]
    public void ReadFileMissingFileReturnsFailure()
    {
        var result = _tool.ReadFileContents("missing.txt");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("File not found within the configured allowlisted roots: 'missing.txt'.", result.Error);
    }

    [TestMethod]
    public void ReadFileOutsideSandboxReturnsAccessDenied()
    {
        var result = _tool.ReadFileContents("..\\outside.txt");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Access denied: '..\\outside.txt' is outside the configured allowlisted roots.", result.Error);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    public void ReadFileWithNullOrWhitespacePathReturnsFailure(string relativePath)
    {
        var result = _tool.ReadFileContents(relativePath!);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Path cannot be empty.", result.Error);
    }
}