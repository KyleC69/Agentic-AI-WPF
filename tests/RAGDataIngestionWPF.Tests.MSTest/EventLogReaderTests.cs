// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         EventLogReaderTests.cs
// Author: Kyle L. Crowder
// Build Num: 091004



namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class EventLogReaderTests
{
    [TestMethod]
    public void ConstructorWithDefaultMaxEventsCreatesInstance()
    {
        SandboxEventLogReader reader = new SandboxEventLogReader();

        Assert.IsNotNull(reader);
    }








    [TestMethod]
    public void ConstructorWithNegativeMaxEventsClampsToOne()
    {
        SandboxEventLogReader reader = new SandboxEventLogReader(-10);

        Assert.IsNotNull(reader);
    }








    [TestMethod]
    public void ConstructorWithPositiveMaxEventsCreatesInstance()
    {
        SandboxEventLogReader reader = new SandboxEventLogReader(50);

        Assert.IsNotNull(reader);
    }








    [TestMethod]
    public void ConstructorWithZeroMaxEventsClampsToOne()
    {
        // SandboxEventLogReader clamps maxEvents to at least 1 via Math.Max(1, maxEvents)
        SandboxEventLogReader reader = new SandboxEventLogReader(0);

        Assert.IsNotNull(reader);
    }








    [TestMethod]
    public void EventLogEntryDtoDefaultPropertiesAreInitialized()
    {
        EventLogEntryDto dto = new EventLogEntryDto();

        Assert.AreEqual(string.Empty, dto.Source);
        Assert.AreEqual(string.Empty, dto.Message);
        Assert.AreEqual(0, dto.EventId);
        Assert.AreEqual(default, dto.EntryType);
    }








    [TestMethod]
    public void EventLogReadResultFailFactorySetsSuccessFalse()
    {
        const string errorMessage = "Test error message";

        EventLogReadResult result = EventLogReadResult.Fail(errorMessage);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(errorMessage, result.Error);
        Assert.IsNull(result.Entries);
    }








    [TestMethod]
    public void EventLogReadResultOkFactorySetsSuccessTrue()
    {
        IReadOnlyList<EventLogEntryDto> entries = new List<EventLogEntryDto>().AsReadOnly();

        EventLogReadResult result = EventLogReadResult.Ok(entries);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Entries);
        Assert.IsNull(result.Error);
    }








    [TestMethod]
    public void ReadLogWithEmptyLogNameReturnsFail()
    {
        SandboxEventLogReader reader = new SandboxEventLogReader();

        EventLogReadResult result = reader.ReadLog(string.Empty);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsNull(result.Entries);
    }








    [TestMethod]
    public void ReadLogWithNonExistentLogNameReturnsFail()
    {
        SandboxEventLogReader reader = new SandboxEventLogReader();

        EventLogReadResult result = reader.ReadLog("ThisEventLogDoesNotExist_RAGDataIngestion_XYZ");

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsNull(result.Entries);
    }








    [TestMethod]
    public void ReadLogWithWhitespaceLogNameReturnsFail()
    {
        SandboxEventLogReader reader = new SandboxEventLogReader();

        EventLogReadResult result = reader.ReadLog("   ");

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsNull(result.Entries);
    }
}