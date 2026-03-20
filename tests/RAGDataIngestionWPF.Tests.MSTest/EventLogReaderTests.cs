using DataIngestionLib.ToolFunctions;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class EventLogReaderTests
{
    [TestMethod]
    public void EventLogReadResultFailSetsErrorAndSuccessFalse()
    {
        EventLogReadResult result = EventLogReadResult.Fail("boom");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("boom", result.Error);
        Assert.IsNull(result.Entries);
    }

    [TestMethod]
    public void EventLogReadResultOkSetsEntriesAndSuccessTrue()
    {
        IReadOnlyList<EventLogEntryDto> entries =
        [
            new EventLogEntryDto
            {
                EventId = 1,
                Source = "src",
                Message = "msg"
            }
        ];

        EventLogReadResult result = EventLogReadResult.Ok(entries);

        Assert.IsTrue(result.Success);
        Assert.AreSame(entries, result.Entries);
        Assert.IsNull(result.Error);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    public void ReadLogWithInvalidNameReturnsFailure(string logName)
    {
        SandboxEventLogReader reader = new();

        ToolResult<IReadOnlyList<EventLogEntryDto>> result = reader.ReadLog(logName!);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Log name cannot be empty.", result.Error);
    }

    [TestMethod]
    public void ReadLogWithMissingLogReturnsFailure()
    {
        SandboxEventLogReader reader = new();

        ToolResult<IReadOnlyList<EventLogEntryDto>> result = reader.ReadLog($"NoSuchLog-{Guid.NewGuid():N}");

        Assert.IsFalse(result.Success);
        StringAssert.StartsWith(result.Error, "Event log '");
        StringAssert.Contains(result.Error, "does not exist.");
    }
}
