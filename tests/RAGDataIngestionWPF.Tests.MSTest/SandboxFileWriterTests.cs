// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         SandboxFileWriterTests.cs
// Author: Kyle L. Crowder
// Build Num: 182419

namespace RAGDataIngestionWPF.Tests.MSTest;




[TestClass]
public class SandboxFileWriterTests
{
    private string _sandboxRoot = string.Empty;

    [TestInitialize]
    public void Initialize()
    {
        _sandboxRoot = Path.Combine(Path.GetTempPath(), "writer-tool-tests", Guid.NewGuid().ToString("N"));
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


}



