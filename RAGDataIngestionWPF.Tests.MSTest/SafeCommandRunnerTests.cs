// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF.Tests.MSTest
//  File:         SafeCommandRunnerTests.cs
//   Author: Kyle L. Crowder



using System.IO;

using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;




/// <summary>
///     Unit tests for <see cref="SafeCommandRunner" /> covering allowlist enforcement,
///     command dispatch, sandbox path restrictions, and input validation.
/// </summary>
[TestClass]
public class SafeCommandRunnerTests
{
    private string _sandboxDir = string.Empty;




    [TestInitialize]
    public void SetUp()
    {
        _sandboxDir = Path.Combine(Path.GetTempPath(), $"SafeCommandRunnerTests_{Guid.NewGuid():N}");
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
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Run_WithNullOrWhitespaceInput_ReturnsNoCommandProvided(string? input)
    {
        // Arrange
        SafeCommandRunner runner = new(_sandboxDir);

        // Act
        string result = runner.Run(input!);

        // Assert
        Assert.AreEqual("No command provided.", result);
    }




    [TestMethod]
    public void Run_WithDisallowedCommand_ReturnsNotAllowedMessage()
    {
        // Arrange
        SafeCommandRunner runner = new(_sandboxDir);

        // Act
        string result = runner.Run("rm -rf /");

        // Assert
        StringAssert.Contains(result, "not allowed");
    }




    [TestMethod]
    public void Run_EchoCommand_ReturnsArguments()
    {
        // Arrange
        SafeCommandRunner runner = new(_sandboxDir);

        // Act
        string result = runner.Run("echo hello world");

        // Assert
        Assert.AreEqual("hello world", result);
    }




    [TestMethod]
    public void Run_LsCommand_ReturnsSandboxFileNames()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_sandboxDir, "alpha.txt"), "a");
        File.WriteAllText(Path.Combine(_sandboxDir, "beta.txt"), "b");

        SafeCommandRunner runner = new(_sandboxDir);

        // Act
        string result = runner.Run("ls");

        // Assert
        StringAssert.Contains(result, "alpha.txt");
        StringAssert.Contains(result, "beta.txt");
    }




    [TestMethod]
    public void Run_CatCommand_WithExistingFile_ReturnsFileContent()
    {
        // Arrange
        const string fileName = "read_me.txt";
        const string expectedContent = "cat test content";
        File.WriteAllText(Path.Combine(_sandboxDir, fileName), expectedContent);

        SafeCommandRunner runner = new(_sandboxDir);

        // Act
        string result = runner.Run($"cat {fileName}");

        // Assert
        Assert.AreEqual(expectedContent, result);
    }




    [TestMethod]
    public void Run_CatCommand_WithNonexistentFile_ReturnsFileNotFound()
    {
        // Arrange
        SafeCommandRunner runner = new(_sandboxDir);

        // Act
        string result = runner.Run("cat ghost_file.txt");

        // Assert
        Assert.AreEqual("File not found.", result);
    }




    [TestMethod]
    public void Run_CatCommand_WithPathTraversal_ReturnsDenied()
    {
        // Arrange
        SafeCommandRunner runner = new(_sandboxDir);

        // Act — attempt to read outside the sandbox
        string result = runner.Run("cat ../../sensitive.txt");

        // Assert
        Assert.AreEqual("Access denied.", result);
    }




    [TestMethod]
    public void Run_EchoWithNoArgs_ReturnsEmptyString()
    {
        // Arrange
        SafeCommandRunner runner = new(_sandboxDir);

        // Act
        string result = runner.Run("echo");

        // Assert
        Assert.AreEqual(string.Empty, result);
    }
}
