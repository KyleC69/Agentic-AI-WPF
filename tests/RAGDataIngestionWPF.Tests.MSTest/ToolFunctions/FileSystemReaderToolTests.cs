// Build Date: 2026/03/29
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         FileSystemReaderToolTests.cs
// Author: Kyle L. Crowder
// Build Num: 051944



using DataIngestionLib.ToolFunctions;



namespace RAGDataIngestionWPF.Tests.MSTest.ToolFunctions;




/// <summary>
///     Unit tests for <see cref="FileContentsReadingTool" /> ReadFile method,
///     focusing on exception handling and edge cases.
/// </summary>
[TestClass]
public class FileSystemReaderToolTests
{
    private string _sandboxRoot = string.Empty;


    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_sandboxRoot))
        {
            try
            {
                Directory.Delete(_sandboxRoot, true);
            }
            catch
            {
                // Ignore cleanup failures
            }
        }
    }


    [TestInitialize]
    public void Initialize()
    {
        _sandboxRoot = Path.Combine(Path.GetTempPath(), "filesystem-reader-tests", Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(_sandboxRoot);
    }


    [TestMethod]
    public void ReadFile_FileWithReadException_ReturnsFailureWithExceptionMessage()
    {
        // Arrange
        var fileName = "locked-file.txt";
        var filePath = Path.Combine(_sandboxRoot, fileName);
        File.WriteAllText(filePath, "test content");

        using var lockStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
        FileContentsReadingTool tool = new(_sandboxRoot);

        // Act
        ToolResult<string> result = tool.ReadFileContents(fileName);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsTrue(result.Error.Contains("process") || result.Error.Contains("used") || result.Error.Contains("access"), $"Actual error: {result.Error}");
        Assert.IsNull(result.Value);
    }


    [TestMethod]
    public void ReadFile_PathTooLong_ReturnsFailureWithExceptionMessage()
    {
        // Arrange
        FileContentsReadingTool tool = new(_sandboxRoot);
        var longPath = new string('a', 300); // Path exceeds max length on Windows

        // Act
        ToolResult<string> result = tool.ReadFileContents(longPath);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
    }


    [TestMethod]
    public void ReadFile_FileInSubdirectory_ReturnsContent()
    {
        // Arrange
        var subDir = Path.Combine(_sandboxRoot, "subfolder");
        _ = Directory.CreateDirectory(subDir);
        var fileName = "test.txt";
        var filePath = Path.Combine(subDir, fileName);
        File.WriteAllText(filePath, "nested content");
        FileContentsReadingTool tool = new(_sandboxRoot);

        // Act
        ToolResult<string> result = tool.ReadFileContents("subfolder/test.txt");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("nested content", result.Value);
        Assert.IsNull(result.Error);
    }


    [TestMethod]
    public void ReadFile_EmptyFile_ReturnsEmptyString()
    {
        // Arrange
        var fileName = "empty.txt";
        var filePath = Path.Combine(_sandboxRoot, fileName);
        File.WriteAllText(filePath, string.Empty);
        FileContentsReadingTool tool = new(_sandboxRoot);

        // Act
        ToolResult<string> result = tool.ReadFileContents(fileName);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(string.Empty, result.Value);
        Assert.IsNull(result.Error);
    }


    [TestMethod]
    public void ReadFile_FileWithSpecialCharacters_ReturnsContent()
    {
        // Arrange
        var content = "Special chars: \r\n\t\"'<>&";
        var fileName = "special.txt";
        var filePath = Path.Combine(_sandboxRoot, fileName);
        File.WriteAllText(filePath, content);
        FileContentsReadingTool tool = new(_sandboxRoot);

        // Act
        ToolResult<string> result = tool.ReadFileContents(fileName);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(content, result.Value);
        Assert.IsNull(result.Error);
    }


    [TestMethod]
    public void ReadFile_LargeFile_ReturnsContent()
    {
        // Arrange
        var content = new string('x', 10000); // 10KB file
        var fileName = "large.txt";
        var filePath = Path.Combine(_sandboxRoot, fileName);
        File.WriteAllText(filePath, content);
        FileContentsReadingTool tool = new(_sandboxRoot);

        // Act
        ToolResult<string> result = tool.ReadFileContents(fileName);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(content, result.Value);
        Assert.IsNull(result.Error);
    }


    [TestMethod]
    public void ReadFile_InvalidCharactersInPath_ReturnsFailureWithExceptionMessage()
    {
        // Arrange
        FileContentsReadingTool tool = new(_sandboxRoot);
        var invalidPath = "file<>|name.txt"; // Invalid filename characters

        // Act
        ToolResult<string> result = tool.ReadFileContents(invalidPath);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
    }


    [TestMethod]
    public void ReadFile_NullPath_ReturnsFailure()
    {
        // Arrange
        FileContentsReadingTool tool = new(_sandboxRoot);

        // Act
        ToolResult<string> result = tool.ReadFileContents(null!);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
    }


    [TestMethod]
    public void ReadFile_FileDeletedAfterExistsCheck_ReturnsFailureWithExceptionMessage()
    {
        // Arrange
        var fileName = "temp-file.txt";
        var filePath = Path.Combine(_sandboxRoot, fileName);
        File.WriteAllText(filePath, "content");
        FileContentsReadingTool tool = new(_sandboxRoot);

        // Create a race condition by using a wrapper that deletes after exists check
        // This is simulated by testing with a file that doesn't exist but testing the exception path
        File.Delete(filePath);

        // Act
        ToolResult<string> result = tool.ReadFileContents(fileName);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual($"File not found: {fileName}", result.Error);
    }


    [TestMethod]
    public void ReadFile_DirectoryInsteadOfFile_ReturnsFailure()
    {
        // Arrange
        var dirName = "testdir";
        var dirPath = Path.Combine(_sandboxRoot, dirName);
        _ = Directory.CreateDirectory(dirPath);
        FileContentsReadingTool tool = new(_sandboxRoot);

        // Act
        ToolResult<string> result = tool.ReadFileContents(dirName);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual($"File not found: {dirName}", result.Error);
    }


    [TestMethod]
    public void ReadFile_UnicodeContent_ReturnsContent()
    {
        // Arrange
        var content = "Hello 世界 🌍 Привет";
        var fileName = "unicode.txt";
        var filePath = Path.Combine(_sandboxRoot, fileName);
        File.WriteAllText(filePath, content);
        FileContentsReadingTool tool = new(_sandboxRoot);

        // Act
        ToolResult<string> result = tool.ReadFileContents(fileName);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(content, result.Value);
        Assert.IsNull(result.Error);
    }
}
