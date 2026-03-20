// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         SandboxFileReaderTests.cs
// Author: Kyle L. Crowder
// Build Num: 182419



using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;




[TestClass]
public class SandboxFileReaderTests
{
	private string _sandboxRoot = string.Empty;

	[TestInitialize]
	public void Initialize()
	{
		_sandboxRoot = Path.Combine(Path.GetTempPath(), "reader-tool-tests", Guid.NewGuid().ToString("N"));
		_ = Directory.CreateDirectory(_sandboxRoot);
	}

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
		Assert.ThrowsExactly<ArgumentException>(() => _ = new FileSystemReaderTool(sandboxRoot!));
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("  ")]
	public void ReadFileWithNullOrWhitespacePathReturnsFailure(string relativePath)
	{
		FileSystemReaderTool tool = new(_sandboxRoot);

		ToolResult<string> result = tool.ReadFile(relativePath!);

		Assert.IsFalse(result.Success);
		Assert.AreEqual("Path cannot be empty.", result.Error);
	}

	[TestMethod]
	public void ReadFileOutsideSandboxReturnsAccessDenied()
	{
		FileSystemReaderTool tool = new(_sandboxRoot);

		ToolResult<string> result = tool.ReadFile("..\\outside.txt");

		Assert.IsFalse(result.Success);
		Assert.AreEqual("Access denied: path is outside the sandbox.", result.Error);
	}

	[TestMethod]
	public void ReadFileMissingFileReturnsFailure()
	{
		FileSystemReaderTool tool = new(_sandboxRoot);

		ToolResult<string> result = tool.ReadFile("missing.txt");

		Assert.IsFalse(result.Success);
		Assert.AreEqual("File not found: missing.txt", result.Error);
	}

	[TestMethod]
	public void ReadFileExistingFileReturnsContent()
	{
		var filePath = Path.Combine(_sandboxRoot, "sample.txt");
		File.WriteAllText(filePath, "hello reader");
		FileSystemReaderTool tool = new(_sandboxRoot);

		ToolResult<string> result = tool.ReadFile("sample.txt");

		Assert.IsTrue(result.Success);
		Assert.AreEqual("hello reader", result.Value);
		Assert.IsNull(result.Error);
	}
}



