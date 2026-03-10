// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF.Tests.MSTest
//  File:         SandboxFileReaderTests.cs
//   Author: Kyle L. Crowder



using System.IO;

using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;




/// <summary>
///     Unit tests for <see cref="SandboxFileReader" /> verifying path validation,
///     sandbox boundary enforcement, and file read behavior.
/// </summary>
[TestClass]
public class SandboxFileReaderTests
{
    private string _sandboxDir = string.Empty;




    [TestInitialize]
    public void SetUp()
    {
        _sandboxDir = Path.Combine(Path.GetTempPath(), $"SandboxFileReaderTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_sandboxDir);
    }




    [TestCleanup]
    public void TearDown()
    {
        if (Directory.Exists(_sandboxDir))
        {
            Directory.Delete(_sandboxDir, recursive: true);
        }
    }




    [TestMethod]
    public void ReadFile_WithValidRelativePath_ReturnsOkWithContent()
    {
        // Arrange
        const string relativePath = "test.txt";
        const string expectedContent = "Hello, sandbox!";
        File.WriteAllText(Path.Combine(_sandboxDir, relativePath), expectedContent);

        SandboxFileReader reader = new(_sandboxDir);

        // Act
        ToolResult<string> result = reader.ReadFile(relativePath);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(expectedContent, result.Value);
        Assert.IsNull(result.Error);
    }




    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void ReadFile_WithNullOrWhitespacePath_ReturnsFail(string? relativePath)
    {
        // Arrange
        SandboxFileReader reader = new(_sandboxDir);

        // Act
        ToolResult<string> result = reader.ReadFile(relativePath!);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsNull(result.Value);
    }




    [TestMethod]
    public void ReadFile_WithPathOutsideSandbox_ReturnsFail()
    {
        // Arrange
        SandboxFileReader reader = new(_sandboxDir);

        // Act — attempt path traversal
        ToolResult<string> result = reader.ReadFile("../../etc/passwd");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsNull(result.Value);
    }




    [TestMethod]
    public void ReadFile_WhenFileDoesNotExist_ReturnsFail()
    {
        // Arrange
        SandboxFileReader reader = new(_sandboxDir);

        // Act
        ToolResult<string> result = reader.ReadFile("nonexistent_file.txt");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsNull(result.Value);
    }




    [TestMethod]
    public void ReadFile_WithNestedRelativePath_ReturnsOkWithContent()
    {
        // Arrange
        string subDir = Path.Combine(_sandboxDir, "subdir");
        Directory.CreateDirectory(subDir);
        const string expectedContent = "nested content";
        File.WriteAllText(Path.Combine(subDir, "nested.txt"), expectedContent);

        SandboxFileReader reader = new(_sandboxDir);

        // Act
        ToolResult<string> result = reader.ReadFile(@"subdir/nested.txt");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(expectedContent, result.Value);
    }




    [TestMethod]
    public void ReadFile_WithEmptyFileContent_ReturnsOkWithEmptyString()
    {
        // Arrange
        const string relativePath = "empty.txt";
        File.WriteAllText(Path.Combine(_sandboxDir, relativePath), string.Empty);

        SandboxFileReader reader = new(_sandboxDir);

        // Act
        ToolResult<string> result = reader.ReadFile(relativePath);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(string.Empty, result.Value);
    }
}
