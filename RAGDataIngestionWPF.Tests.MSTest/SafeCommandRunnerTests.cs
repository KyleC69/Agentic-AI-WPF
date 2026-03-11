// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         SafeCommandRunnerTests.cs
// Author: Kyle L. Crowder
// Build Num: 105606



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








    [TestMethod]
    public void Run_CatCommand_WithExistingFile_ReturnsFileContent()
    {
        // Arrange
        const string fileName = "read_me.txt";
        const string expectedContent = "cat test content";
        File.WriteAllText(Path.Combine(_sandboxDir, fileName), expectedContent);

        SafeCommandRunner runner = new(_sandboxDir);

        // Act
        var result = runner.Run($"cat {fileName}");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(expectedContent, result.Value);
    }








    [TestMethod]
    public void Run_CatCommand_WithNonexistentFile_ReturnsFileNotFound()
    {
        // Arrange
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxDir);

        // Act
        var result = runner.Run("cat ghost_file.txt");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("File not found.", result.Error);
    }








    [TestMethod]
    public void Run_CatCommand_WithPathTraversal_ReturnsDenied()
    {
        // Arrange
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxDir);

        // Act — attempt to read outside the sandbox
        var result = runner.Run("cat ../../sensitive.txt");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("Access denied.", result.Error);
    }








    [TestMethod]
    public void Run_EchoCommand_ReturnsArguments()
    {
        // Arrange
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxDir);

        // Act
        var result = runner.Run("echo hello world");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("hello world", result.Value);
    }








    [TestMethod]
    public void Run_EchoWithNoArgs_ReturnsEmptyString()
    {
        // Arrange
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxDir);

        // Act
        var result = runner.Run("echo");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(string.Empty, result.Value);
    }








    [TestMethod]
    public void Run_LsCommand_ReturnsSandboxFileNames()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_sandboxDir, "alpha.txt"), "a");
        File.WriteAllText(Path.Combine(_sandboxDir, "beta.txt"), "b");

        SafeCommandRunner runner = new SafeCommandRunner(_sandboxDir);

        // Act
        var result = runner.Run("ls");

        // Assert
        Assert.IsTrue(result.Success);
        StringAssert.Contains(result.Value!, "alpha.txt");
        StringAssert.Contains(result.Value!, "beta.txt");
    }








    [TestMethod]
    public void Run_WithDisallowedCommand_ReturnsNotAllowedMessage()
    {
        // Arrange
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxDir);

        // Act
        var result = runner.Run("rm -rf /");

        // Assert
        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.Error!, "not allowed");
    }








    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Run_WithNullOrWhitespaceInput_ReturnsNoCommandProvided(string? input)
    {
        // Arrange
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxDir);

        // Act
        var result = runner.Run(input!);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("No command provided.", result.Error);
    }








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
}