// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         RegistryReaderToolTests.cs
// Author: Kyle L. Crowder
// Build Num: 213000



using DataIngestionLib.ToolFunctions;

using Microsoft.Extensions.Logging.Abstractions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class RegistryReaderToolTests
{
    [TestMethod]
    public void ConstructorWithNullLoggerFactoryThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = new RegistryReaderTool(null!));
    }








    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    public void ReadStringValueWithEmptyPathReturnsFailure(string keyPath)
    {
        RegistryReaderTool tool = new(NullLoggerFactory.Instance);

        var result = tool.ReadStringValue(keyPath!);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Registry key path cannot be null or empty.", result.Error);
    }








    [TestMethod]
    public void ReadStringValueWithEmptySubkeyReturnsFailure()
    {
        RegistryReaderTool tool = new RegistryReaderTool(NullLoggerFactory.Instance);

        var result = tool.ReadStringValue("HKEY_CURRENT_USER\\");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Registry subkey path cannot be empty.", result.Error);
    }








    [TestMethod]
    public void ReadStringValueWithInvalidFormatReturnsFailure()
    {
        RegistryReaderTool tool = new RegistryReaderTool(NullLoggerFactory.Instance);

        var result = tool.ReadStringValue("HKEY_CURRENT_USER");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Invalid registry key path format.", result.Error);
    }








    [TestMethod]
    public void ReadStringValueWithMissingKeyReturnsFailure()
    {
        RegistryReaderTool tool = new RegistryReaderTool(NullLoggerFactory.Instance);
        var path = $"HKEY_CURRENT_USER\\Software\\RAGDataIngestionWPF\\{Guid.NewGuid():N}\\MissingValue";

        var result = tool.ReadStringValue(path);

        Assert.IsFalse(result.Success);
        StringAssert.StartsWith(result.Error, "Registry key not found:");
    }








    [TestMethod]
    public void ReadStringValueWithUnsupportedHiveReturnsFailure()
    {
        RegistryReaderTool tool = new RegistryReaderTool(NullLoggerFactory.Instance);

        var result = tool.ReadStringValue("HKEY_UNKNOWN\\Software\\Value");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Unsupported registry hive: HKEY_UNKNOWN", result.Error);
    }
}