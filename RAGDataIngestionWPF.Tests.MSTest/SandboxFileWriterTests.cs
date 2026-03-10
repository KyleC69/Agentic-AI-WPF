// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF.Tests.MSTest
//  File:         SandboxFileWriterTests.cs
//   Author: Kyle L. Crowder



using System.IO;

using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;




/// <summary>
///     Unit tests for <see cref="SandboxFileWriter" /> verifying path validation,
///     sandbox boundary enforcement, content persistence, and directory creation.
/// </summary>
[TestClass]
public class SandboxFileWriterTests
{
    private string _sandboxDir = string.Empty;




    [TestInitialize]
    public void SetUp()
    {
        _sandboxDir = Path.Combine(Path.GetTempPath(), $"SandboxFileWriterTests_{Guid.NewGuid():N}");
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
    public void WriteFile_WithValidPathAndContent_WritesContentToDisk()
    {
        // Arrange
        const string relativePath = "output.txt";
        const string content = "test content";
        SandboxFileWriter writer = new(_sandboxDir);

        // Act
        writer.WriteFile(relativePath, content);

        // Assert
        string fullPath = Path.Combine(_sandboxDir, relativePath);
        Assert.IsTrue(File.Exists(fullPath));
        Assert.AreEqual(content, File.ReadAllText(fullPath));
    }




    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void WriteFile_WithNullOrWhitespacePath_ThrowsArgumentException(string? relativePath)
    {
        // Arrange
        SandboxFileWriter writer = new(_sandboxDir);

        // Act / Assert
        Assert.ThrowsExactly<ArgumentException>(() => writer.WriteFile(relativePath!, "data"));
    }




    [TestMethod]
    public void WriteFile_WithPathOutsideSandbox_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        SandboxFileWriter writer = new(_sandboxDir);

        // Act / Assert — path traversal attempt
        Assert.ThrowsExactly<UnauthorizedAccessException>(() =>
            writer.WriteFile("../../escaped.txt", "malicious content"));
    }




    [TestMethod]
    public void WriteFile_WithNullContent_WritesEmptyFile()
    {
        // Arrange
        const string relativePath = "nullcontent.txt";
        SandboxFileWriter writer = new(_sandboxDir);

        // Act
        writer.WriteFile(relativePath, null!);

        // Assert
        string fullPath = Path.Combine(_sandboxDir, relativePath);
        Assert.IsTrue(File.Exists(fullPath));
        Assert.AreEqual(string.Empty, File.ReadAllText(fullPath));
    }




    [TestMethod]
    public void WriteFile_WithNestedPath_CreatesIntermediateDirectories()
    {
        // Arrange
        const string relativePath = @"sub/nested/deep/file.txt";
        const string content = "deep content";
        SandboxFileWriter writer = new(_sandboxDir);

        // Act
        writer.WriteFile(relativePath, content);

        // Assert
        string fullPath = Path.Combine(_sandboxDir, "sub", "nested", "deep", "file.txt");
        Assert.IsTrue(File.Exists(fullPath));
        Assert.AreEqual(content, File.ReadAllText(fullPath));
    }




    [TestMethod]
    public void WriteFile_OverwritesExistingFile()
    {
        // Arrange
        const string relativePath = "overwrite.txt";
        string fullPath = Path.Combine(_sandboxDir, relativePath);
        File.WriteAllText(fullPath, "original");
        SandboxFileWriter writer = new(_sandboxDir);

        // Act
        writer.WriteFile(relativePath, "updated");

        // Assert
        Assert.AreEqual("updated", File.ReadAllText(fullPath));
    }
}
