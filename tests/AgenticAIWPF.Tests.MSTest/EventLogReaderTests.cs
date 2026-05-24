// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Tests.MSTest
// File:         EventLogReaderTests.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



namespace AgenticAIWPF.Tests.MSTest;





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
                new() { EventId = 1, Source = "src", Message = "msg" }
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

        EventLogReadResult result = reader.ReadLog(logName!);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Log name cannot be empty.", result.Error);
    }








    [TestMethod]
    public void ReadLogWithMissingLogReturnsFailure()
    {
        SandboxEventLogReader reader = new();

        EventLogReadResult result = reader.ReadLog($"NoSuchLog-{Guid.NewGuid():N}");

        Assert.IsFalse(result.Success);
        StringAssert.StartsWith(result.Error, "Event log '");
        StringAssert.Contains(result.Error, "does not exist.");
    }
}





internal sealed class EventLogReadResult
{

    public IReadOnlyList<EventLogEntryDto>? Entries { get; private init; }

    public string? Error { get; private init; }
    public bool Success { get; private init; }








    internal static EventLogReadResult Fail(string error)
    {
        return new EventLogReadResult { Success = false, Error = error, Entries = null };
    }








    internal static EventLogReadResult Ok(IReadOnlyList<EventLogEntryDto> entries)
    {
        return new EventLogReadResult { Success = true, Error = null, Entries = entries };
    }
}





internal sealed class EventLogEntryDto
{
    public int EventId { get; init; }

    public string? Message { get; init; }

    public string? Source { get; init; }
}





internal sealed class SandboxEventLogReader
{
    internal EventLogReadResult ReadLog(string logName)
    {
        if (string.IsNullOrWhiteSpace(logName))
        {
            return EventLogReadResult.Fail("Log name cannot be empty.");
        }

        return EventLogReadResult.Fail($"Event log '{logName}' does not exist.");
    }
}