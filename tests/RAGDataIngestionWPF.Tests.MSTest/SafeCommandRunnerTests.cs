// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         SafeCommandRunnerTests.cs
// Author: Kyle L. Crowder
// Build Num: 091006



using DataIngestionLib.ToolFunctions;

using JetBrains.Annotations;




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
    public void RunCatCommandWithExistingFileReturnsFileContent()
    {
        // Arrange
        const string fileName = "read_me.txt";
        const string expectedContent = "cat test content";
        File.WriteAllText(Path.Combine(_sandboxDir, fileName), expectedContent);

        SafeCommandRunner runner = new SafeCommandRunner(_sandboxDir);

        // Act
        var result = runner.Run($"cat {fileName}");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(expectedContent, result.Value);
    }








    [TestMethod]
    public void RunCatCommandWithNonexistentFileReturnsFileNotFound()
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
    public void RunCatCommandWithPathTraversalReturnsDenied()
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
    public void RunEchoCommandReturnsArguments()
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
    public void RunEchoWithNoArgsReturnsEmptyString()
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
    public void RunLsCommandReturnsSandboxFileNames()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_sandboxDir, "alpha.txt"), "a");
        File.WriteAllText(Path.Combine(_sandboxDir, "beta.txt"), "b");

        SafeCommandRunner runner = new SafeCommandRunner(_sandboxDir);

        // Act
        var result = runner.Run("ls");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.Contains("alpha.txt", result.Value!);
        Assert.Contains("beta.txt", result.Value!);
    }








    [TestMethod]
    public void RunWithDisallowedCommandReturnsNotAllowedMessage()
    {
        // Arrange
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxDir);

        // Act
        var result = runner.Run("rm -rf /");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.Contains("not allowed", result.Error!);
    }








    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void RunWithNullOrWhitespaceInputReturnsNoCommandProvided([NotNull] string input)
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
        _ = Directory.CreateDirectory(_sandboxDir);
    }








    [TestCleanup]
    public void TearDown()
    {
        if (Directory.Exists(_sandboxDir))
        {
            Directory.Delete(_sandboxDir, true);
        }
    }
}