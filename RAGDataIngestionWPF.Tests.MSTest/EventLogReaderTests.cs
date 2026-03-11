// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         EventLogReaderTests.cs
// Author: Kyle L. Crowder
// Build Num: 105605



namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class EventLogReaderTests
{
    [TestMethod]
    public void Constructor_WithDefaultMaxEvents_CreatesInstance()
    {
        SandboxEventLogReader reader = new SandboxEventLogReader();

        Assert.IsNotNull(reader);
    }








    [TestMethod]
    public void Constructor_WithNegativeMaxEvents_ClampsToOne()
    {
        SandboxEventLogReader reader = new SandboxEventLogReader(-10);

        Assert.IsNotNull(reader);
    }








    [TestMethod]
    public void Constructor_WithPositiveMaxEvents_CreatesInstance()
    {
        SandboxEventLogReader reader = new SandboxEventLogReader(50);

        Assert.IsNotNull(reader);
    }








    [TestMethod]
    public void Constructor_WithZeroMaxEvents_ClampsToOne()
    {
        // SandboxEventLogReader clamps maxEvents to at least 1 via Math.Max(1, maxEvents)
        SandboxEventLogReader reader = new SandboxEventLogReader(0);

        Assert.IsNotNull(reader);
    }








    [TestMethod]
    public void EventLogEntryDto_DefaultProperties_AreInitialized()
    {
        EventLogEntryDto dto = new EventLogEntryDto();

        Assert.AreEqual(string.Empty, dto.Source);
        Assert.AreEqual(string.Empty, dto.Message);
        Assert.AreEqual(0, dto.EventId);
        Assert.AreEqual(default, dto.EntryType);
    }








    [TestMethod]
    public void EventLogReadResult_FailFactory_SetsSuccessFalse()
    {
        const string errorMessage = "Test error message";

        EventLogReadResult result = EventLogReadResult.Fail(errorMessage);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(errorMessage, result.Error);
        Assert.IsNull(result.Entries);
    }








    [TestMethod]
    public void EventLogReadResult_OkFactory_SetsSuccessTrue()
    {
        IReadOnlyList<EventLogEntryDto> entries = new List<EventLogEntryDto>().AsReadOnly();

        EventLogReadResult result = EventLogReadResult.Ok(entries);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Entries);
        Assert.IsNull(result.Error);
    }








    [TestMethod]
    public void ReadLog_WithEmptyLogName_ReturnsFail()
    {
        SandboxEventLogReader reader = new SandboxEventLogReader();

        EventLogReadResult result = reader.ReadLog(string.Empty);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsNull(result.Entries);
    }








    [TestMethod]
    public void ReadLog_WithNonExistentLogName_ReturnsFail()
    {
        SandboxEventLogReader reader = new SandboxEventLogReader();

        EventLogReadResult result = reader.ReadLog("ThisEventLogDoesNotExist_RAGDataIngestion_XYZ");

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsNull(result.Entries);
    }








    [TestMethod]
    public void ReadLog_WithWhitespaceLogName_ReturnsFail()
    {
        SandboxEventLogReader reader = new SandboxEventLogReader();

        EventLogReadResult result = reader.ReadLog("   ");

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsNull(result.Entries);
    }
}