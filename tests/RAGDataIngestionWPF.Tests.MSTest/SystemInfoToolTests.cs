using DataIngestionLib.ToolFunctions;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class SystemInfoToolTests
{
    [TestMethod]
    public void GetInfoReturnsSnapshotWithExpectedFields()
    {
        ToolResult<SystemInfoSnapshot> result = SystemInfoTool.GetInfo();

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Value.Os));
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Value.MachineName));
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Value.DotNetVersion));
        Assert.IsTrue(result.Value.ProcessorCount > 0);
        Assert.IsNull(result.Error);
    }
}
