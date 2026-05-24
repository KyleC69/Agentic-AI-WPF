// Build Date: 2026/04/13
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Tests.MSTest
// File:         EventErrorsToolTests.cs
// Author: GitHub Copilot
// Build Num: 203501



using AgentAILib.ToolFunctions.OSTools;
using System.Reflection;





namespace AgenticAIWPF.Tests.MSTest;





[TestClass]
public class EventErrorsToolTests
{
    private const string EVENT_ERRORS_TOOL_TYPE_NAME = "AgentAILib.ToolFunctions.OSTools.EventErrorsTool, AgentAILib";





    [TestMethod]
    public void BuildCriticalAndErrorQueryUsesExpectedLevelFilterAndWindow()
    {
        var query = (string)InvokeStaticMethod("BuildCriticalAndErrorQuery", 6)!;

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

        var result = (IReadOnlyList<string>)InvokeStaticMethod("GetCandidateLogs", logs)!;
        var maxLogsToScan = (int)GetPublicConstFieldValue("MAX_LOGS_TO_SCAN")!;

        Assert.AreEqual(maxLogsToScan, result.Count);
        Assert.AreEqual("System", result[0]);
        Assert.AreEqual("Application", result[1]);
        Assert.AreEqual("Setup", result[2]);
    }





    [TestMethod]
    [DataRow(0)]
    [DataRow(25)]
    public void ReadRecentCriticalAndErrorEventsWithInvalidLookbackReturnsFailure(int lookbackHours)
    {
        var tool = CreateToolInstance();

        var result = InvokeInstanceMethod(tool, "ReadRecentCriticalAndErrorEvents", lookbackHours)!;
        var success = (bool)result.GetType().GetProperty("Success")!.GetValue(result)!;
        var error = (string)result.GetType().GetProperty("Error")!.GetValue(result)!;

        Assert.IsFalse(success);
        Assert.AreEqual("lookbackHours must be between 1 and 24.", error);
    }





    private static object CreateToolInstance()
    {
        var type = ResolveEventErrorsToolType();
        return Activator.CreateInstance(type)!;
    }

    private static object? InvokeStaticMethod(string methodName, params object[] args)
    {
        var type = ResolveEventErrorsToolType();
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)!;
        return method.Invoke(null, args);
    }

    private static object? InvokeInstanceMethod(object instance, string methodName, params object[] args)
    {
        var method = instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance)!;
        return method.Invoke(instance, args);
    }

    private static object? GetPublicConstFieldValue(string fieldName)
    {
        var type = ResolveEventErrorsToolType();
        var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static)!;
        return field.GetRawConstantValue();
    }

    private static Type ResolveEventErrorsToolType()
    {
        return Type.GetType(EVENT_ERRORS_TOOL_TYPE_NAME, throwOnError: true)!;
    }
}