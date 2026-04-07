// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         SandboxFileReaderTests.cs
// Author: Kyle L. Crowder
// Build Num: 213001



using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class SandboxFileReaderTests
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








    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    public void ConstructorWithEmptySandboxRootThrowsArgumentException(string sandboxRoot)
    {
        Assert.ThrowsExactly<ArgumentException>(() => _ = new FileContentsReadingTool(sandboxRoot!));
    }








    [TestInitialize]
    public void Initialize()
    {
        _sandboxRoot = Path.Combine(Path.GetTempPath(), "reader-tool-tests", Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(_sandboxRoot);
    }








    [TestMethod]
    [DataRow("")]
    [DataRow("Nuget")]
    [DataRow("certs")]
    public void Read_Known_Good_Path_(string path)
    {

        _ = new FileContentsReadingTool("E:\\");

        var result = FileContentsReadingTool.ReadFileContents(path);


        Assert.IsNotNull(result.Value);
        Assert.IsTrue(result.Success);
        //    Assert.AreEqual("hello reader", result.Value);
        Assert.IsNull(result.Error);
    }








    [TestMethod]
    public void ReadFileExistingFileReturnsContent()
    {
        var filePath = Path.Combine(_sandboxRoot, "sample.txt");
        File.WriteAllText(filePath, "hello reader");
        _ = new FileContentsReadingTool(_sandboxRoot);

        var result = FileContentsReadingTool.ReadFileContents("sample.txt");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("hello reader", result.Value);
        Assert.IsNull(result.Error);
    }








    [TestMethod]
    public void ReadFileMissingFileReturnsFailure()
    {
        _ = new FileContentsReadingTool(_sandboxRoot);

        var result = FileContentsReadingTool.ReadFileContents("missing.txt");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("File not found: missing.txt", result.Error);
    }








    [TestMethod]
    public void ReadFileOutsideSandboxReturnsAccessDenied()
    {
        _ = new FileContentsReadingTool(_sandboxRoot);

        var result = FileContentsReadingTool.ReadFileContents("..\\outside.txt");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Access denied: path is outside the sandbox.", result.Error);
    }








    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    public void ReadFileWithNullOrWhitespacePathReturnsFailure(string relativePath)
    {
        _ = new FileContentsReadingTool(_sandboxRoot);

        var result = FileContentsReadingTool.ReadFileContents(relativePath!);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Path cannot be empty.", result.Error);
    }
}