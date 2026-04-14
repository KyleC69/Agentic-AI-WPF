// Build Date: 2026/04/13
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         EventErrorsToolTests.cs
// Author: GitHub Copilot
// Build Num: 203501



using DataIngestionLib.ToolFunctions.OSTools;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class EventErrorsToolTests
{
    [TestMethod]
    public void BuildCriticalAndErrorQueryUsesExpectedLevelFilterAndWindow()
    {
        var query = EventErrorsTool.BuildCriticalAndErrorQuery(6);

        StringAssert.Contains(query, "Level=1 or Level=2");
        StringAssert.Contains(query, "21600000");
    }





    [TestMethod]
    public void GetCandidateLogsPrioritizesCoreLogsAndCapsToTwentyFive()
    {
        var logs = Enumerable.Range(0, 40)
            .Select(index => $"Custom-{index:D2}")
            .Concat(["Application", "System", "Setup", "Vendor/Admin", "Vendor/Operational"])
            .ToList();

        var result = EventErrorsTool.GetCandidateLogs(logs);

        Assert.AreEqual(EventErrorsTool.MAX_LOGS_TO_SCAN, result.Count);
        Assert.AreEqual("System", result[0]);
        Assert.AreEqual("Application", result[1]);
        Assert.AreEqual("Setup", result[2]);
    }





    [TestMethod]
    [DataRow(0)]
    [DataRow(25)]
    public void ReadRecentCriticalAndErrorEventsWithInvalidLookbackReturnsFailure(int lookbackHours)
    {
        EventErrorsTool tool = new();

        var result = tool.ReadRecentCriticalAndErrorEvents(lookbackHours);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("lookbackHours must be between 1 and 24.", result.Error);
    }
}